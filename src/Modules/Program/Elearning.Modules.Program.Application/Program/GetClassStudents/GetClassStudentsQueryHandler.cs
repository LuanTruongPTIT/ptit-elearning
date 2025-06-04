using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetClassStudents;

internal sealed class GetClassStudentsQueryHandler : IQueryHandler<GetClassStudentsQuery, GetClassStudentsResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetClassStudentsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<GetClassStudentsResponse>> Handle(GetClassStudentsQuery request, CancellationToken cancellationToken)
  {
    try
    {
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

      // Build WHERE clause for search
      var searchCondition = string.IsNullOrEmpty(request.SearchTerm)
          ? ""
          : "AND (u.full_name ILIKE @SearchTerm OR u.email ILIKE @SearchTerm)";

      // Build ORDER BY clause
      var orderBy = request.SortBy?.ToLower() switch
      {
        "name" => $"sb.student_name {request.SortOrder}",
        "email" => $"sb.email {request.SortOrder}",
        "progress" => $"overall_progress {request.SortOrder}",
        "lastaccessed" => $"last_accessed {request.SortOrder}",
        _ => "sb.student_name ASC"
      };

      // 1. Get total count
      var countSql = $@"
                SELECT COUNT(DISTINCT sp.student_id)
                FROM programs.table_student_programs sp
                JOIN users.table_users u ON sp.student_id = u.id
                WHERE sp.program_id = (SELECT program_id FROM programs.classes WHERE id = @ClassId)
                {searchCondition}";

      var totalCount = await connection.QuerySingleAsync<int>(countSql, new
      {
        request.ClassId,
        SearchTerm = $"%{request.SearchTerm}%"
      });

      // 2. Get students with pagination
      var offset = (request.Page - 1) * request.PageSize;
      var studentsSql = $@"
                WITH student_base AS (
                    SELECT 
                        sp.student_id,
                        u.full_name as student_name,
                        u.email,
                        u.avatar_url,
                        COALESCE(MAX(scp.last_accessed), u.created_at) as last_accessed
                    FROM programs.table_student_programs sp
                    JOIN users.table_users u ON sp.student_id = u.id
                    LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                    WHERE sp.program_id = (SELECT program_id FROM programs.classes WHERE id = @ClassId)
                    {searchCondition}
                    GROUP BY sp.student_id, u.full_name, u.email, u.avatar_url, u.created_at
                ),
                student_progress AS (
                    SELECT 
                        student_id,
                        AVG(progress_percentage) as overall_progress,
                        COUNT(CASE WHEN progress_percentage >= 100 THEN 1 END) as completed_courses,
                        COUNT(CASE WHEN progress_percentage > 0 AND progress_percentage < 100 THEN 1 END) as in_progress_courses,
                        COUNT(CASE WHEN COALESCE(progress_percentage, 0) = 0 THEN 1 END) as not_started_courses
                    FROM programs.table_student_course_progress
                    GROUP BY student_id
                ),
                assignment_stats AS (
                    SELECT 
                        asub.student_id,
                        COUNT(DISTINCT a.id) as total_assignments,
                        COUNT(DISTINCT CASE WHEN asub.status = 'submitted' THEN asub.id END) as completed_assignments,
                        AVG(CASE WHEN asub.grade IS NOT NULL THEN asub.grade END) as average_grade
                    FROM programs.table_assignments a
                    JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                    JOIN programs.classes c ON tac.class_id = c.id
                    LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                    WHERE c.id = @ClassId
                    GROUP BY asub.student_id
                )
                SELECT 
                    sb.student_id,
                    sb.student_name,
                    sb.email,
                    sb.avatar_url,
                    COALESCE(sp.overall_progress, 0) as overall_progress,
                    COALESCE(sp.completed_courses, 0) as completed_courses,
                    COALESCE(sp.in_progress_courses, 0) as in_progress_courses,
                    COALESCE(sp.not_started_courses, 0) as not_started_courses,
                    COALESCE(ast.total_assignments, 0) as total_assignments,
                    COALESCE(ast.completed_assignments, 0) as completed_assignments,
                    COALESCE(ast.average_grade, 0) as average_grade,
                    sb.last_accessed,
                    CASE 
                        WHEN sb.last_accessed >= NOW() - INTERVAL '7 days' THEN 'active'
                        ELSE 'inactive'
                    END as status
                FROM student_base sb
                LEFT JOIN student_progress sp ON sb.student_id = sp.student_id
                LEFT JOIN assignment_stats ast ON sb.student_id = ast.student_id
                ORDER BY {orderBy}
                LIMIT @PageSize OFFSET @Offset";

      var students = await connection.QueryAsync(studentsSql, new
      {
        request.ClassId,
        SearchTerm = $"%{request.SearchTerm}%",
        PageSize = request.PageSize,
        Offset = offset
      });

      // 3. Get course progress for each student
      var studentIds = students.Select(s => s.student_id).ToList();

      // Calculate total pages first
      var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

      if (!studentIds.Any())
      {
        // No students found, return empty response
        var emptyResponse = new GetClassStudentsResponse
        {
          Students = new List<ClassStudentDetail>(),
          TotalCount = totalCount,
          Page = request.Page,
          PageSize = request.PageSize,
          TotalPages = totalPages,
          HasNextPage = request.Page < totalPages,
          HasPreviousPage = request.Page > 1
        };
        return Result.Success(emptyResponse);
      }

      // Create IN clause for SQL query
      var studentIdsString = string.Join(",", studentIds.Select(id => $"'{id}'"));

      var courseProgressSql = $@"
                SELECT 
                    scp.student_id,
                    tac.id as course_id,
                    tac.course_class_name as course_name,
                    scp.progress_percentage as progress,
                    scp.status,
                    scp.last_accessed
                FROM programs.table_student_course_progress scp
                JOIN programs.table_teaching_assign_courses tac ON scp.teaching_assign_course_id = tac.id
                JOIN programs.classes c ON tac.class_id = c.id
                WHERE c.id = @ClassId AND scp.student_id IN ({studentIdsString})";

      var courseProgressResult = await connection.QueryAsync(courseProgressSql, new
      {
        request.ClassId
      });

      var courseProgressList = courseProgressResult.ToList();
      var courseProgressByStudent = new Dictionary<Guid, List<StudentCourseProgress>>();

      foreach (var cp in courseProgressList)
      {
        if (!courseProgressByStudent.ContainsKey(cp.student_id))
        {
          courseProgressByStudent[cp.student_id] = new List<StudentCourseProgress>();
        }

        courseProgressByStudent[cp.student_id].Add(new StudentCourseProgress
        {
          CourseId = cp.course_id.ToString(),
          CourseName = cp.course_name,
          Progress = Math.Round((double)cp.progress, 2),
          Status = cp.status,
          LastAccessed = cp.last_accessed
        });
      }

      // 4. Build response
      var studentDetails = students.Select(s => new ClassStudentDetail
      {
        StudentId = s.student_id.ToString(),
        StudentName = s.student_name,
        Email = s.email,
        AvatarUrl = s.avatar_url ?? "/api/placeholder/40/40",
        OverallProgress = Math.Round((double)s.overall_progress, 2),
        CompletedCourses = (int)s.completed_courses,
        InProgressCourses = (int)s.in_progress_courses,
        NotStartedCourses = (int)s.not_started_courses,
        TotalAssignments = (int)s.total_assignments,
        CompletedAssignments = (int)s.completed_assignments,
        PendingAssignments = (int)s.total_assignments - (int)s.completed_assignments,
        AverageGrade = Math.Round((double)s.average_grade, 2),
        LastAccessed = s.last_accessed,
        Status = s.status,
        CourseProgress = courseProgressByStudent.ContainsKey(s.student_id)
              ? courseProgressByStudent[s.student_id]
              : new List<StudentCourseProgress>()
      }).ToList();

      var response = new GetClassStudentsResponse
      {
        Students = studentDetails,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize,
        TotalPages = totalPages,
        HasNextPage = request.Page < totalPages,
        HasPreviousPage = request.Page > 1
      };

      return Result.Success(response);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<GetClassStudentsResponse>(
                new Error("GetClassStudents.Error", ex.Message, ErrorType.Failure));
    }
  }
}
