using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetStudentDetail;

internal sealed class GetStudentDetailQueryHandler : IQueryHandler<GetStudentDetailQuery, GetStudentDetailResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetStudentDetailQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<GetStudentDetailResponse>> Handle(GetStudentDetailQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

      // 1. Get basic student information
      var studentSql = @"
                SELECT 
                    u.id as student_id,
                    u.full_name as student_name,
                    u.email,
                    u.avatar_url,
                    u.phone_number,
                    u.address,
                    u.date_of_birth,
                    u.gender,
                    sp.created_at as enrollment_date,
                    p.name as program_name,
                    d.name as department,
                    COALESCE(MAX(scp.last_accessed), u.created_at) as last_accessed
                FROM users.table_users u
                JOIN programs.table_student_programs sp ON u.id = sp.student_id
                JOIN programs.table_programs p ON sp.program_id = p.id
                LEFT JOIN programs.table_departments d ON p.department_id = d.id
                LEFT JOIN programs.table_student_course_progress scp ON u.id = scp.student_id
                WHERE u.id = @StudentId
                GROUP BY u.id, u.full_name, u.email, u.avatar_url, u.phone_number, u.address, 
                         u.date_of_birth, u.gender, sp.created_at, p.name, d.name, u.created_at";

      var studentInfo = await connection.QueryFirstOrDefaultAsync(studentSql, new { StudentId = Guid.Parse(request.StudentId) });

      if (studentInfo == null)
      {
        return Result.Failure<GetStudentDetailResponse>(
            new Error("GetStudentDetail.NotFound", "Student not found", ErrorType.NotFound));
      }

      // 2. Get course progress statistics
      var courseProgressSql = @"
                SELECT 
                    scp.teaching_assign_course_id as course_id,
                    tac.course_class_name as course_name,
                    scp.progress_percentage as progress,
                    scp.status,
                    scp.last_accessed,
                    COUNT(DISTINCT cl.id) as total_lectures,
                    COUNT(DISTINCT CASE WHEN slp.is_completed = true THEN cl.id END) as completed_lectures,
                    COUNT(DISTINCT a.id) as total_assignments,
                    COUNT(DISTINCT CASE WHEN asub.status = 'submitted' THEN a.id END) as completed_assignments,
                    AVG(asub.grade) as current_grade
                FROM programs.table_student_course_progress scp
                JOIN programs.table_teaching_assign_courses tac ON scp.teaching_assign_course_id = tac.id
                LEFT JOIN programs.table_lectures cl ON tac.id = cl.teaching_assign_course_id
                LEFT JOIN programs.table_student_lecture_progress slp ON cl.id = slp.lecture_id AND slp.student_id = scp.student_id
                LEFT JOIN programs.table_assignments a ON tac.id = a.teaching_assign_course_id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id AND asub.student_id = scp.student_id
                WHERE scp.student_id = @StudentId
                GROUP BY scp.teaching_assign_course_id, tac.course_class_name, scp.progress_percentage, scp.status, scp.last_accessed";

      var courseProgressList = await connection.QueryAsync(courseProgressSql, new { StudentId = Guid.Parse(request.StudentId) });

      // 3. Get overall statistics
      var overallProgress = courseProgressList.Any()
          ? courseProgressList.Average(cp => (decimal)cp.progress)
          : 0m;

      var completedCourses = courseProgressList.Count(cp => (decimal)cp.progress >= 100);
      var inProgressCourses = courseProgressList.Count(cp => (decimal)cp.progress > 0 && (decimal)cp.progress < 100);
      var notStartedCourses = courseProgressList.Count(cp => (decimal)cp.progress == 0);

      // 4. Get assignment statistics
      var assignmentStatsSql = @"
                SELECT 
                    COUNT(DISTINCT a.id) as total_assignments,
                    COUNT(DISTINCT CASE WHEN asub.status = 'submitted' THEN a.id END) as completed_assignments,
                    AVG(asub.grade) as average_grade
                FROM programs.table_assignments a
                JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                JOIN programs.classes c ON tac.class_id = c.id
                JOIN programs.table_student_programs sp ON c.program_id = sp.program_id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id AND asub.student_id = @StudentId
                WHERE sp.student_id = @StudentId";

      var assignmentStats = await connection.QueryFirstOrDefaultAsync(assignmentStatsSql, new { StudentId = Guid.Parse(request.StudentId) });

      // 5. Get recent assignments
      var recentAssignmentsSql = @"
                SELECT 
                    a.id as assignment_id,
                    a.title as assignment_title,
                    tac.course_class_name as course_name,
                    a.deadline,
                    asub.submitted_at,
                    asub.grade as score,
                    a.max_score,
                    CASE 
                        WHEN asub.id IS NULL THEN 'not_submitted'
                        WHEN asub.grade IS NULL THEN 'submitted'
                        ELSE 'graded'
                    END as status,
                    CASE WHEN asub.submitted_at > a.deadline THEN true ELSE false END as is_late
                FROM programs.table_assignments a
                JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                JOIN programs.classes c ON tac.class_id = c.id
                JOIN programs.table_student_programs sp ON c.program_id = sp.program_id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id AND asub.student_id = @StudentId
                WHERE sp.student_id = @StudentId
                ORDER BY a.deadline DESC
                LIMIT 10";

      var recentAssignmentsList = await connection.QueryAsync(recentAssignmentsSql, new { StudentId = Guid.Parse(request.StudentId) });

      // 6. Get recent activities from multiple sources
      var recentActivitiesSql = @"
                WITH recent_activities AS (
                    -- Course access activities (from course progress)
                    SELECT 
                        'course_access' as activity_type,
                        'Accessed course content' as description,
                        scp.last_accessed as timestamp,
                        tac.course_class_name as course_name,
                        NULL::decimal as score,
                        scp.last_accessed as activity_time
                    FROM programs.table_student_course_progress scp
                    JOIN programs.table_teaching_assign_courses tac ON scp.teaching_assign_course_id = tac.id
                    WHERE scp.student_id = @StudentId 
                    AND scp.last_accessed IS NOT NULL
                    
                    UNION ALL
                    
                    -- Assignment submission activities
                    SELECT 
                        'assignment_submit' as activity_type,
                        CONCAT('Submitted assignment: ', a.title) as description,
                        asub.submitted_at as timestamp,
                        tac.course_class_name as course_name,
                        asub.grade as score,
                        asub.submitted_at as activity_time
                    FROM programs.table_assignment_submissions asub
                    JOIN programs.table_assignments a ON asub.assignment_id = a.id
                    JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                    WHERE asub.student_id = @StudentId 
                    AND asub.submitted_at IS NOT NULL
                    
                    UNION ALL
                    
                    -- Lecture completion activities
                    SELECT 
                        'lecture_complete' as activity_type,
                        CONCAT('Completed lecture: ', l.title) as description,
                        slp.last_accessed as timestamp,
                        tac.course_class_name as course_name,
                        NULL::decimal as score,
                        slp.last_accessed as activity_time
                    FROM programs.table_student_lecture_progress slp
                    JOIN programs.table_lectures l ON slp.lecture_id = l.id
                    JOIN programs.table_teaching_assign_courses tac ON l.teaching_assign_course_id = tac.id
                    WHERE slp.student_id = @StudentId 
                    AND slp.is_completed = true
                    AND slp.last_accessed IS NOT NULL
                )
                SELECT 
                    activity_type,
                    description,
                    timestamp,
                    course_name,
                    score
                FROM recent_activities
                WHERE activity_time IS NOT NULL
                ORDER BY activity_time DESC
                LIMIT 10";

      var recentActivitiesData = await connection.QueryAsync(recentActivitiesSql, new { StudentId = Guid.Parse(request.StudentId) });

      // Build recent activities response
      var recentActivitiesList = recentActivitiesData.Select(activity => new StudentActivityDetailResponse(
          activity.activity_type?.ToString() ?? "",
          activity.description?.ToString() ?? "",
          activity.timestamp != null ? Convert.ToDateTime(activity.timestamp) : DateTime.Now,
          activity.course_name?.ToString(),
          activity.score != null ? (decimal?)Convert.ToDecimal(activity.score) : null
      )).ToList();

      // Build course progress response
      var courseProgressResponse = courseProgressList.Select(cp => new StudentCourseDetailResponse(
          cp.course_id.ToString(),
          cp.course_name?.ToString() ?? "",
          Math.Round((decimal)cp.progress, 2),
          cp.status?.ToString() ?? "not_started",
          cp.last_accessed != null ? (DateTime?)Convert.ToDateTime(cp.last_accessed) : null,
          (int)(cp.completed_lectures ?? 0),
          (int)(cp.total_lectures ?? 0),
          (int)(cp.completed_assignments ?? 0),
          (int)(cp.total_assignments ?? 0),
          cp.current_grade != null ? (decimal?)Math.Round((decimal)cp.current_grade, 2) : null
      )).ToList();

      // Build assignment response
      var assignmentResponse = recentAssignmentsList.Select(a => new StudentAssignmentDetailResponse(
          a.assignment_id.ToString(),
          a.assignment_title?.ToString() ?? "",
          a.course_name?.ToString() ?? "",
          Convert.ToDateTime(a.deadline),
          a.submitted_at != null ? (DateTime?)Convert.ToDateTime(a.submitted_at) : null,
          a.score != null ? (decimal?)Math.Round((decimal)a.score, 2) : null,
          Math.Round((decimal)a.max_score, 2),
          a.status?.ToString() ?? "not_submitted",
          Convert.ToBoolean(a.is_late)
      )).ToList();

      // Determine student status
      var status = studentInfo.last_accessed >= DateTime.Now.AddDays(-7) ? "active" : "inactive";

      var response = new GetStudentDetailResponse(
          studentInfo.student_id.ToString(),
          studentInfo.student_name?.ToString() ?? "",
          studentInfo.email?.ToString() ?? "",
          studentInfo.avatar_url?.ToString(),
          studentInfo.phone_number?.ToString(),
          studentInfo.address?.ToString(),
          studentInfo.date_of_birth != null ? (DateTime?)Convert.ToDateTime(studentInfo.date_of_birth) : null,
          studentInfo.gender?.ToString(),
          Convert.ToDateTime(studentInfo.enrollment_date),
          studentInfo.program_name?.ToString() ?? "",
          studentInfo.department?.ToString(),
          Math.Round(overallProgress, 2),
          completedCourses,
          inProgressCourses,
          notStartedCourses,
          (int)(assignmentStats?.total_assignments ?? 0),
          (int)(assignmentStats?.completed_assignments ?? 0),
          (int)(assignmentStats?.total_assignments ?? 0) - (int)(assignmentStats?.completed_assignments ?? 0),
          Math.Round((decimal)(assignmentStats?.average_grade ?? 0), 2),
          Convert.ToDateTime(studentInfo.last_accessed),
          status,
          courseProgressResponse,
          assignmentResponse,
          recentActivitiesList
      );

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<GetStudentDetailResponse>(
                new Error("GetStudentDetail.Error", ex.Message, ErrorType.Failure));
    }
  }
}
