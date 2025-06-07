using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Courses.Queries.GetCourseById;

internal sealed class GetCourseByIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetCourseByIdQuery, GetCourseByIdResponse>
{
    public async Task<Result<GetCourseByIdResponse>> Handle(
        GetCourseByIdQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        try
        {
            var courseInstanceId = Guid.Parse(request.CourseId);

            // Main course instance query - from table_teaching_assign_courses
            var courseSql = """
                SELECT 
                    tac.id,
                    CONCAT(c.name, ' - ', cl.class_name) as name,
                    c.code,
                    tac.description,
                    COALESCE(tac.thumbnail_url, '') as thumbnail_url,
                    tac.status,
                    tac.created_at,
                    tac.updated_at,
                    COALESCE(d.name, 'Chưa xác định') as department,
                    COALESCE(u.full_name, 'Chưa phân công') as teacher_name,
                    COALESCE(u.avatar_url, '') as teacher_avatar,
                120 AS duration,
                    ROUND(CAST(4.0 + (RANDOM() * 1.0) AS numeric), 1) as rating
                FROM programs.table_teaching_assign_courses tac
                JOIN programs.table_courses c ON tac.course_id = c.id
                JOIN programs.classes cl ON tac.class_id = cl.id
                LEFT JOIN programs.table_programs p ON cl.program_id = p.id
                LEFT JOIN programs.table_departments d ON p.department_id = d.id
                LEFT JOIN users.table_users u ON tac.teacher_id = u.id
                WHERE tac.id = @courseInstanceId
            """;

            var course = await connection.QueryFirstOrDefaultAsync(courseSql, new { courseInstanceId });
            if (course == null)
            {
                return Result.Failure<GetCourseByIdResponse>(
                    Error.NotFound("Course.NotFound", "Course instance not found"));
            }

            // Course statistics query
            var statisticsSql = """
                SELECT 
                    COUNT(DISTINCT sp.student_id) as students_enrolled,
                    50 as max_students,
                    COALESCE(AVG(scp.progress_percentage), 0) as completion_rate,
                    -- Mock calculations with proper ROUND casting
                    ROUND(CAST(75 + (RANDOM() * 20) AS numeric), 2) as average_score,
                    ROUND(CAST(5 + (RANDOM() * 10) AS numeric), 2) as dropout_rate,
                    FLOOR(20 + (RANDOM() * 30)) as total_lessons,
                    FLOOR(15 + (RANDOM() * 25)) as completed_lessons,
                    FLOOR(10 + (RANDOM() * 15)) as total_assignments,
                    ROUND(CAST(75 + (RANDOM() * 20) AS numeric), 2) as assignment_completion_rate,
                    FLOOR(50 + (RANDOM() * 100)) as average_study_time
                FROM programs.table_teaching_assign_courses tac
                JOIN programs.classes cl ON tac.class_id = cl.id
                LEFT JOIN programs.table_student_programs sp ON cl.program_id = sp.program_id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                    AND tac.id = scp.teaching_assign_course_id
                WHERE tac.id = @courseInstanceId
                GROUP BY max_students
            """;

            var statistics = await connection.QueryFirstOrDefaultAsync(statisticsSql, new { courseInstanceId });

            // Enrolled students query
            var enrolledStudentsSql = """
                SELECT 
                    u.id as student_id,
                    u.full_name as StudentName,
                    COALESCE(u.avatar_url, '') as avatar,
                    COALESCE(scp.progress_percentage, 0) as progress,
                    ROUND(CAST(70 + (RANDOM() * 30) AS numeric), 2) as score,
                    CASE 
                        WHEN scp.progress_percentage >= 100 THEN 'Completed'
                        WHEN scp.progress_percentage > 0 THEN 'In Progress'
                        ELSE 'Not Started'
                    END as status,
                    COALESCE(scp.last_accessed, NOW()) as LastAccessed,
                    sp.created_at as EnrollmentDate
                FROM programs.table_teaching_assign_courses tac
                JOIN programs.classes cl ON tac.class_id = cl.id
                JOIN programs.table_student_programs sp ON cl.program_id = sp.program_id
                JOIN users.table_users u ON sp.student_id = u.id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                    AND tac.id = scp.teaching_assign_course_id
                WHERE tac.id = @courseInstanceId
                ORDER BY sp.created_at DESC
                LIMIT 20
            """;

            var enrolledStudents = await connection.QueryAsync<EnrolledStudent>(enrolledStudentsSql, new { courseInstanceId });

            // Generate mock data for charts
            var enrollmentTrends = GenerateEnrollmentTrends();
            var progressDistributions = GenerateProgressDistributions();
            var courseContents = GenerateCourseContents();
            var response = new GetCourseByIdResponse
            {
                Id = course.id.ToString(),
                Name = course.name ?? "",
                Code = course.code ?? "",
                Description = course.description ?? "",
                ThumbnailUrl = course.thumbnail_url ?? "",
                Status = course.status ?? "",
                CreatedAt = course.created_at?.ToString("yyyy-MM-dd") ?? "",
                UpdatedAt = course.updated_at?.ToString("yyyy-MM-dd") ?? "",
                Department = course.department ?? "",
                TeacherName = course.teacher_name ?? "",
                TeacherAvatar = course.teacher_avatar ?? "",
                Duration = (int)(course.duration ?? 0),
                Rating = (decimal)(course.rating ?? 0),
                StudentsEnrolled = (int)(statistics?.students_enrolled ?? 0),
                MaxStudents = (int)(statistics?.max_students ?? 0),
                CompletionRate = (decimal)(statistics?.completion_rate ?? 0),
                PerformanceStats = new CoursePerformanceStats
                {
                    AverageScore = (decimal)(statistics?.average_score ?? 0),
                    CompletionRate = (decimal)(statistics?.completion_rate ?? 0),
                    DropoutRate = (decimal)(statistics?.dropout_rate ?? 0),
                    TotalLessons = (int)(statistics?.total_lessons ?? 0),
                    CompletedLessons = (int)(statistics?.completed_lessons ?? 0),
                    TotalAssignments = (int)(statistics?.total_assignments ?? 0),
                    AssignmentCompletionRate = (decimal)(statistics?.assignment_completion_rate ?? 0),
                    AverageStudyTime = (int)(statistics?.average_study_time ?? 0),
                    DifficultyLevel = "Intermediate"
                },
                EnrolledStudents = enrolledStudents.ToList(),
                EnrollmentTrends = enrollmentTrends,
                ProgressDistributions = progressDistributions,
                CourseContents = courseContents
            };


            return Result.Success(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetCourseByIdQueryHandler: {ex}");
            return Result.Failure<GetCourseByIdResponse>(
                Error.Failure("GetCourseById.Error", ex.Message));
        }
    }

    private static List<MonthlyEnrollment> GenerateEnrollmentTrends()
    {
        var trends = new List<MonthlyEnrollment>();
        var random = new Random();

        for (int i = 5; i >= 0; i--)
        {
            var date = DateTime.Now.AddMonths(-i);
            trends.Add(new MonthlyEnrollment
            {
                Month = date.ToString("MM/yyyy"),
                NewEnrollments = 5 + random.Next(20),
                Completions = 2 + random.Next(15),
                Dropouts = random.Next(5),
                AverageScore = 70 + (decimal)(random.NextDouble() * 25)
            });
        }

        return trends;
    }

    private static List<ProgressDistribution> GenerateProgressDistributions()
    {
        var random = new Random();
        return new List<ProgressDistribution>
        {
            new() { Range = "0-25%", Count = 5 + random.Next(10), Percentage = 10 + random.Next(20), Color = "#EF4444" },
            new() { Range = "26-50%", Count = 8 + random.Next(15), Percentage = 15 + random.Next(25), Color = "#F59E0B" },
            new() { Range = "51-75%", Count = 12 + random.Next(20), Percentage = 20 + random.Next(30), Color = "#3B82F6" },
            new() { Range = "76-100%", Count = 15 + random.Next(25), Percentage = 25 + random.Next(35), Color = "#10B981" }
        };
    }

    private static List<CourseContent> GenerateCourseContents()
    {
        var contents = new List<CourseContent>();
        var random = new Random();
        var types = new[] { "lesson", "assignment", "quiz" };

        for (int i = 1; i <= 10; i++)
        {
            var type = types[i % 3];
            contents.Add(new CourseContent
            {
                Id = $"content_{i}",
                Title = $"Content {i}",
                Type = type,
                Duration = 30 + random.Next(60),
                CompletionRate = 60 + random.Next(40),
                Status = "active",
                Order = i
            });
        }

        return contents;
    }
}
