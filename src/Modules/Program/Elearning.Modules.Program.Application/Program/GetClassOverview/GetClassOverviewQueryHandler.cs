using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetClassOverview;

internal sealed class GetClassOverviewQueryHandler : IQueryHandler<GetClassOverviewQuery, GetClassOverviewResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetClassOverviewQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<GetClassOverviewResponse>> Handle(GetClassOverviewQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

      // 1. Get class basic info
      var classInfoSql = @"
                SELECT 
                    c.id as class_id,
                    c.class_name,
                    p.name as program_name
                FROM programs.classes c
                JOIN programs.table_programs p ON c.program_id = p.id
                WHERE c.id = @ClassId";

      var classInfo = await connection.QueryFirstOrDefaultAsync(classInfoSql, new { request.ClassId });

      if (classInfo == null)
      {
        return Result.Failure<GetClassOverviewResponse>(
            new Error("GetClassOverview.ClassNotFound", "Class not found", ErrorType.NotFound));
      }

      // 2. Get students in class
      var studentsSql = @"
                SELECT 
                    sp.student_id,
                    u.full_name as student_name,
                    u.email,
                    COALESCE(avg_progress.overall_progress, 0) as overall_progress,
                    COALESCE(course_stats.completed_courses, 0) as completed_courses,
                    COALESCE(course_stats.in_progress_courses, 0) as in_progress_courses,
                    COALESCE(MAX(scp.last_accessed), u.created_at) as last_accessed
                FROM programs.table_student_programs sp
                JOIN users.table_users u ON sp.student_id = u.id
                LEFT JOIN (
                    SELECT 
                        student_id,
                        AVG(progress_percentage) as overall_progress
                    FROM programs.table_student_course_progress
                    GROUP BY student_id
                ) avg_progress ON sp.student_id = avg_progress.student_id
                LEFT JOIN (
                    SELECT 
                        student_id,
                        COUNT(CASE WHEN progress_percentage >= 100 THEN 1 END) as completed_courses,
                        COUNT(CASE WHEN progress_percentage > 0 AND progress_percentage < 100 THEN 1 END) as in_progress_courses
                    FROM programs.table_student_course_progress
                    GROUP BY student_id
                ) course_stats ON sp.student_id = course_stats.student_id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                WHERE sp.program_id = (SELECT program_id FROM programs.classes WHERE id = @ClassId)
                GROUP BY sp.student_id, u.full_name, u.email, u.created_at, 
                         avg_progress.overall_progress, course_stats.completed_courses, course_stats.in_progress_courses";

      var students = await connection.QueryAsync(studentsSql, new { request.ClassId });
      var studentsList = students.ToList();

      // 3. Get course progress for the class
      var courseProgressSql = @"
                SELECT 
                    tac.id as course_id,
                    tac.course_class_name as course_name,
                    COALESCE(AVG(scp.progress_percentage), 0) as average_progress,
                    COUNT(CASE WHEN scp.progress_percentage >= 100 THEN 1 END) as students_completed,
                    COUNT(CASE WHEN scp.progress_percentage > 0 AND scp.progress_percentage < 100 THEN 1 END) as students_in_progress,
                    COUNT(CASE WHEN COALESCE(scp.progress_percentage, 0) = 0 THEN 1 END) as students_not_started
                FROM programs.table_teaching_assign_courses tac
                JOIN programs.classes c ON tac.class_id = c.id
                LEFT JOIN programs.table_student_course_progress scp ON tac.id = scp.teaching_assign_course_id
                LEFT JOIN programs.table_student_programs sp ON scp.student_id = sp.student_id AND sp.program_id = c.program_id
                WHERE c.id = @ClassId AND tac.status = 'active'
                GROUP BY tac.id, tac.course_class_name";

      var courseProgress = await connection.QueryAsync(courseProgressSql, new { request.ClassId });

      // 4. Get assignment statistics
      var assignmentStatsSql = @"
                SELECT 
                    COUNT(CASE WHEN asub.status = 'submitted' THEN 1 END) as completed_assignments,
                    COUNT(CASE WHEN a.id IS NOT NULL AND asub.id IS NULL THEN 1 END) as pending_assignments
                FROM programs.table_assignments a
                JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                JOIN programs.classes c ON tac.class_id = c.id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                LEFT JOIN programs.table_student_programs sp ON asub.student_id = sp.student_id AND sp.program_id = c.program_id
                WHERE c.id = @ClassId";

      var assignmentStats = await connection.QueryFirstOrDefaultAsync(assignmentStatsSql, new { request.ClassId });

      // 5. Build response
      var totalStudents = studentsList.Count;
      var activeStudents = studentsList.Count(s => s.last_accessed >= DateTime.UtcNow.AddDays(-7));
      var averageProgress = studentsList.Any() ? studentsList.Average(s => (double)s.overall_progress) : 0;

      var response = new GetClassOverviewResponse
      {
        ClassId = classInfo.class_id.ToString(),
        ClassName = classInfo.class_name,
        ProgramName = classInfo.program_name,
        TotalStudents = totalStudents,
        ActiveStudents = activeStudents,
        InactiveStudents = totalStudents - activeStudents,
        AverageProgress = Math.Round(averageProgress, 2),
        TotalCourses = courseProgress.Count(),
        CompletedAssignments = (int)(assignmentStats?.completed_assignments ?? 0),
        PendingAssignments = (int)(assignmentStats?.pending_assignments ?? 0),
        CourseProgress = courseProgress.Select(cp => new CourseProgressSummary
        {
          CourseId = cp.course_id.ToString(),
          CourseName = cp.course_name,
          AverageProgress = Math.Round((double)cp.average_progress, 2),
          StudentsCompleted = (int)cp.students_completed,
          StudentsInProgress = (int)cp.students_in_progress,
          StudentsNotStarted = (int)cp.students_not_started
        }).ToList(),
        TopPerformers = studentsList
              .OrderByDescending(s => s.overall_progress)
              .Take(5)
              .Select(s => new StudentPerformanceSummary
              {
                StudentId = s.student_id.ToString(),
                StudentName = s.student_name,
                Email = s.email,
                OverallProgress = Math.Round((double)s.overall_progress, 2),
                CompletedCourses = (int)s.completed_courses,
                InProgressCourses = (int)s.in_progress_courses,
                LastAccessed = s.last_accessed
              }).ToList(),
        LowPerformers = studentsList
              .OrderBy(s => s.overall_progress)
              .Take(5)
              .Select(s => new StudentPerformanceSummary
              {
                StudentId = s.student_id.ToString(),
                StudentName = s.student_name,
                Email = s.email,
                OverallProgress = Math.Round((double)s.overall_progress, 2),
                CompletedCourses = (int)s.completed_courses,
                InProgressCourses = (int)s.in_progress_courses,
                LastAccessed = s.last_accessed
              }).ToList()
      };

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<GetClassOverviewResponse>(
                new Error("GetClassOverview.Error", ex.Message, ErrorType.Failure));
    }
  }
}
