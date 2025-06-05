using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetTeacherDashboard;

internal sealed class GetTeacherDashboardQueryHandler : IQueryHandler<GetTeacherDashboardQuery, GetTeacherDashboardResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetTeacherDashboardQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<GetTeacherDashboardResponse>> Handle(GetTeacherDashboardQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

      // 1. Get overall teacher statistics
      var teacherStatsSql = @"
                WITH teacher_classes AS (
                    SELECT DISTINCT c.id as class_id, c.class_name, p.name as program_name, c.program_id
                    FROM programs.classes c
                    INNER JOIN programs.table_programs p ON c.program_id = p.id
                    INNER JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                    WHERE tac.teacher_id = @TeacherId AND c.status = 'active' AND tac.status = 'active'
                ),
                teacher_courses AS (
                    SELECT DISTINCT tac.id as course_id, tac.course_class_name, tc.class_id, tc.class_name, tc.program_name
                    FROM programs.table_teaching_assign_courses tac
                    INNER JOIN teacher_classes tc ON tac.class_id = tc.class_id
                    WHERE tac.teacher_id = @TeacherId AND tac.status = 'active'
                ),
                student_stats AS (
                    SELECT 
                        COUNT(DISTINCT sp.student_id) as total_students,
                        AVG(scp.progress_percentage) as avg_completion_rate
                    FROM teacher_classes tc
                    JOIN programs.table_student_programs sp ON tc.program_id = sp.program_id
                    LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                ),
                assignment_stats AS (
                    SELECT 
                        COUNT(DISTINCT a.id) as total_assignments,
                        COUNT(DISTINCT CASE WHEN asub.status = 'submitted' AND asub.grade IS NOT NULL THEN asub.id END) as completed_assignments,
                        COUNT(DISTINCT CASE WHEN asub.status = 'submitted' AND asub.grade IS NULL THEN asub.id END) as pending_assignments,
                        AVG(asub.grade) as average_grade
                    FROM teacher_courses tc
                    JOIN programs.table_assignments a ON tc.course_id = a.teaching_assign_course_id
                    LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                )
                SELECT 
                    COALESCE(ss.total_students, 0) as total_students,
                    (SELECT COUNT(*) FROM teacher_courses) as total_courses,
                    (SELECT COUNT(*) FROM teacher_classes) as total_classes,
                    COALESCE(ss.avg_completion_rate, 0) as avg_completion_rate,
                    COALESCE(asts.average_grade, 0) as average_grade,
                    COALESCE(asts.completed_assignments, 0) as completed_assignments,
                    COALESCE(asts.pending_assignments, 0) as pending_assignments,
                    COALESCE(asts.total_assignments, 0) as total_assignments
                FROM student_stats ss
                CROSS JOIN assignment_stats asts";

      var teacherStats = await connection.QueryFirstOrDefaultAsync(teacherStatsSql, new { TeacherId = Guid.Parse(request.TeacherId) });

      // 2. Get course overviews
      var courseOverviewSql = @"
                SELECT 
                    tac.id as course_id,
                    tac.course_class_name as course_name,
                    c.class_name,
                    COUNT(DISTINCT sp.student_id) as enrolled_students,
                    AVG(COALESCE(scp.progress_percentage, 0)) as average_progress,
                    COUNT(DISTINCT CASE WHEN scp.progress_percentage >= 100 THEN sp.student_id END) as completed_students,
                    COUNT(DISTINCT CASE WHEN scp.progress_percentage > 0 AND scp.progress_percentage < 100 THEN sp.student_id END) as in_progress_students,
                    COUNT(DISTINCT CASE WHEN COALESCE(scp.progress_percentage, 0) = 0 THEN sp.student_id END) as not_started_students
                FROM programs.table_teaching_assign_courses tac
                JOIN programs.classes c ON tac.class_id = c.id
                LEFT JOIN programs.table_student_programs sp ON c.program_id = sp.program_id
                LEFT JOIN programs.table_student_course_progress scp ON tac.id = scp.teaching_assign_course_id AND sp.student_id = scp.student_id
                WHERE tac.teacher_id = @TeacherId AND tac.status = 'active'
                GROUP BY tac.id, tac.course_class_name, c.class_name
                ORDER BY tac.course_class_name";

      var courseOverviews = await connection.QueryAsync(courseOverviewSql, new { TeacherId = Guid.Parse(request.TeacherId) });

      // 3. Get recent activities
      var recentActivitiesSql = @"
                WITH recent_activities AS (
                    -- Assignment submissions
                    SELECT 
                        'assignment_submit' as activity_type,
                        CONCAT('Submitted assignment: ', a.title) as description,
                        asub.submitted_at as timestamp,
                        u.full_name as student_name,
                        tac.course_class_name as course_name,
                        asub.grade as score,
                        asub.submitted_at as activity_time
                    FROM programs.table_assignment_submissions asub
                    JOIN programs.table_assignments a ON asub.assignment_id = a.id
                    JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                    JOIN users.table_users u ON asub.student_id = u.id
                    WHERE tac.teacher_id = @TeacherId 
                    AND asub.submitted_at IS NOT NULL
                    AND asub.submitted_at >= NOW() - INTERVAL '30 days'
                    
                    UNION ALL
                    
                    -- Lecture completions
                    SELECT 
                        'lecture_complete' as activity_type,
                        CONCAT('Completed lecture: ', l.title) as description,
                        slp.last_accessed as timestamp,
                        u.full_name as student_name,
                        tac.course_class_name as course_name,
                        NULL::decimal as score,
                        slp.last_accessed as activity_time
                    FROM programs.table_student_lecture_progress slp
                    JOIN programs.table_lectures l ON slp.lecture_id = l.id
                    JOIN programs.table_teaching_assign_courses tac ON l.teaching_assign_course_id = tac.id
                    JOIN users.table_users u ON slp.student_id = u.id
                    WHERE tac.teacher_id = @TeacherId 
                    AND slp.is_completed = true
                    AND slp.last_accessed IS NOT NULL
                    AND slp.last_accessed >= NOW() - INTERVAL '30 days'
                )
                SELECT 
                    activity_type,
                    description,
                    timestamp,
                    student_name,
                    course_name,
                    score
                FROM recent_activities
                WHERE activity_time IS NOT NULL
                ORDER BY activity_time DESC
                LIMIT 10";

      var recentActivities = await connection.QueryAsync(recentActivitiesSql, new { TeacherId = Guid.Parse(request.TeacherId) });

      // 4. Get class summaries
      var classSummariesSql = @"
                SELECT 
                    c.id as class_id,
                    c.class_name,
                    p.name as program_name,
                    COUNT(DISTINCT sp.student_id) as student_count,
                    AVG(COALESCE(scp.progress_percentage, 0)) as average_progress,
                    COUNT(DISTINCT tac.id) as course_count
                FROM programs.classes c
                JOIN programs.table_programs p ON c.program_id = p.id
                JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                LEFT JOIN programs.table_student_programs sp ON c.program_id = sp.program_id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                WHERE tac.teacher_id = @TeacherId AND c.status = 'active' AND tac.status = 'active'
                GROUP BY c.id, c.class_name, p.name
                ORDER BY c.class_name";

      var classSummaries = await connection.QueryAsync(classSummariesSql, new { TeacherId = Guid.Parse(request.TeacherId) });

      // Build response
      var response = new GetTeacherDashboardResponse(
          TotalStudents: (int)(teacherStats?.total_students ?? 0),
          TotalCourses: (int)(teacherStats?.total_courses ?? 0),
          TotalClasses: (int)(teacherStats?.total_classes ?? 0),
          AverageCompletionRate: Math.Round((decimal)(teacherStats?.avg_completion_rate ?? 0), 2),
          AverageGrade: Math.Round((decimal)(teacherStats?.average_grade ?? 0), 2),
          CompletedAssignments: (int)(teacherStats?.completed_assignments ?? 0),
          PendingAssignments: (int)(teacherStats?.pending_assignments ?? 0),
          TotalAssignments: (int)(teacherStats?.total_assignments ?? 0),
          CourseOverviews: courseOverviews.Select(co => new TeacherCourseOverview(
              CourseId: co.course_id,
              CourseName: co.course_name?.ToString() ?? "",
              ClassName: co.class_name?.ToString() ?? "",
              EnrolledStudents: (int)(co.enrolled_students ?? 0),
              AverageProgress: Math.Round((decimal)(co.average_progress ?? 0), 2),
              CompletedStudents: (int)(co.completed_students ?? 0),
              InProgressStudents: (int)(co.in_progress_students ?? 0),
              NotStartedStudents: (int)(co.not_started_students ?? 0)
          )).ToList(),
          RecentActivities: recentActivities.Select(ra => new TeacherRecentActivity(
              ActivityType: ra.activity_type?.ToString() ?? "",
              Description: ra.description?.ToString() ?? "",
              Timestamp: ra.timestamp != null ? Convert.ToDateTime(ra.timestamp) : DateTime.Now,
              StudentName: ra.student_name?.ToString() ?? "",
              CourseName: ra.course_name?.ToString() ?? "",
              Score: ra.score != null ? (decimal?)Convert.ToDecimal(ra.score) : null
          )).ToList(),
          ClassSummaries: classSummaries.Select(cs => new TeacherClassSummary(
              ClassId: cs.class_id,
              ClassName: cs.class_name?.ToString() ?? "",
              ProgramName: cs.program_name?.ToString() ?? "",
              StudentCount: (int)(cs.student_count ?? 0),
              AverageProgress: Math.Round((decimal)(cs.average_progress ?? 0), 2),
              CourseCount: (int)(cs.course_count ?? 0)
          )).ToList()
      );

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<GetTeacherDashboardResponse>(
          new Error("GetTeacherDashboard.Error", ex.Message, ErrorType.Failure));
    }
  }
}
