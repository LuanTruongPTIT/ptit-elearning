using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetTeacherClasses;

internal sealed class GetTeacherClassesQueryHandler : IQueryHandler<GetTeacherClassesQuery, List<GetTeacherClassesResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetTeacherClassesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<List<GetTeacherClassesResponse>>> Handle(GetTeacherClassesQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

      // Get all classes that the teacher is teaching
      var classesSql = @"
                SELECT DISTINCT
                    c.id as class_id,
                    c.class_name,
                    p.name as program_name
                FROM programs.classes c
                INNER JOIN programs.table_programs p ON c.program_id = p.id
                INNER JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                WHERE tac.teacher_id = @TeacherId
                AND c.status = 'active'
                AND tac.status = 'active'
                ORDER BY c.class_name";

      var classes = await connection.QueryAsync(classesSql, new { TeacherId = Guid.Parse(request.TeacherId) });
      var classesList = classes.ToList();

      if (!classesList.Any())
      {
        return Result.Success(new List<GetTeacherClassesResponse>());
      }

      var classIds = classesList.Select(c => c.class_id).ToArray();

      // Create IN clause for SQL queries
      var classIdsString = string.Join(",", classIds.Select(id => $"'{id}'"));

      // Get student statistics for all classes
      var studentStatsSql = $@"
                SELECT 
                    c.id as class_id,
                    COUNT(DISTINCT sp.student_id) as total_students,
                    COUNT(DISTINCT CASE 
                        WHEN scp.last_accessed >= NOW() - INTERVAL '7 days' 
                        THEN sp.student_id 
                    END) as active_students,
                    COALESCE(AVG(scp.progress_percentage), 0) as average_progress
                FROM programs.classes c
                LEFT JOIN programs.table_student_programs sp ON c.program_id = sp.program_id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                WHERE c.id IN ({classIdsString})
                GROUP BY c.id";

      var studentStats = await connection.QueryAsync(studentStatsSql);
      var studentStatsDict = studentStats.ToDictionary(s => s.class_id, s => s);

      // Get course statistics for all classes
      var courseStatsSql = $@"
                SELECT 
                    c.id as class_id,
                    COUNT(DISTINCT tac.id) as total_courses
                FROM programs.classes c
                LEFT JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                WHERE c.id IN ({classIdsString})
                AND tac.status = 'active'
                GROUP BY c.id";

      var courseStats = await connection.QueryAsync(courseStatsSql);
      var courseStatsDict = courseStats.ToDictionary(s => s.class_id, s => s);

      // Get assignment statistics for all classes
      var assignmentStatsSql = $@"
                SELECT 
                    c.id as class_id,
                    COUNT(DISTINCT a.id) as total_assignments,
                    COUNT(DISTINCT CASE 
                        WHEN asub.status = 'submitted' AND asub.grade IS NOT NULL 
                        THEN asub.id 
                    END) as completed_assignments,
                    COUNT(DISTINCT CASE 
                        WHEN asub.status = 'submitted' AND asub.grade IS NULL 
                        THEN asub.id 
                    END) as pending_assignments
                FROM programs.classes c
                LEFT JOIN programs.table_teaching_assign_courses tac ON c.id = tac.class_id
                LEFT JOIN programs.table_assignments a ON tac.id = a.teaching_assign_course_id
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                WHERE c.id IN ({classIdsString})
                GROUP BY c.id";

      var assignmentStats = await connection.QueryAsync(assignmentStatsSql);
      var assignmentStatsDict = assignmentStats.ToDictionary(s => s.class_id, s => s);

      // Build the response
      var responses = new List<GetTeacherClassesResponse>();

      foreach (var classItem in classesList)
      {
        var studentStat = studentStatsDict.ContainsKey(classItem.class_id) ? studentStatsDict[classItem.class_id] : null;
        var courseStat = courseStatsDict.ContainsKey(classItem.class_id) ? courseStatsDict[classItem.class_id] : null;
        var assignmentStat = assignmentStatsDict.ContainsKey(classItem.class_id) ? assignmentStatsDict[classItem.class_id] : null;

        var totalStudents = (int)(studentStat?.total_students ?? 0);
        var activeStudents = (int)(studentStat?.active_students ?? 0);
        var inactiveStudents = totalStudents - activeStudents;

        responses.Add(new GetTeacherClassesResponse(
            ClassId: classItem.class_id,
            ClassName: classItem.class_name,
            ProgramName: classItem.program_name,
            TotalStudents: totalStudents,
            ActiveStudents: activeStudents,
            InactiveStudents: inactiveStudents,
            AverageProgress: Math.Round((decimal)(studentStat?.average_progress ?? 0), 2),
            TotalCourses: (int)(courseStat?.total_courses ?? 0),
            CompletedAssignments: (int)(assignmentStat?.completed_assignments ?? 0),
            PendingAssignments: (int)(assignmentStat?.pending_assignments ?? 0),
            CourseProgress: new List<CourseProgressResponse>(), // Will be empty for list view
            TopPerformers: new List<StudentPerformanceResponse>(), // Will be empty for list view
            LowPerformers: new List<StudentPerformanceResponse>() // Will be empty for list view
        ));
      }

      return Result.Success(responses);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<List<GetTeacherClassesResponse>>(
            Error.Failure("GetTeacherClasses", $"An error occurred: {ex.Message}")
        );
    }
  }
}
