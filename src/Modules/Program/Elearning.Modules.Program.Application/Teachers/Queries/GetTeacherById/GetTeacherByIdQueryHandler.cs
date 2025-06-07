using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Teachers.Queries.GetTeacherById;

internal sealed class GetTeacherByIdQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetTeacherByIdQuery, GetTeacherByIdResponse>
{
    public async Task<Result<GetTeacherByIdResponse>> Handle(
        GetTeacherByIdQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        try
        {
            var teacherSql = """
                SELECT 
                    u.id,
                    u.full_name as name,
                    u.email,
                    u.phone_number,
                    u.created_at as join_date,
                    u.account_status as status,
                    COALESCE(u.avatar_url, '') as avatar,
                    COALESCE(u.address, '') as address,
                    COALESCE(d.name, 'Chưa xác định') as department,
                    'Software Engineering' as specialization,
                    ROUND(CAST(4.0 + (RANDOM() * 1.0) AS numeric), 1) as rating
                FROM users.table_users u
                JOIN users.table_user_roles ur ON u.id = ur.user_id
                LEFT JOIN programs.table_teaching_assign_courses tac ON u.id = tac.teacher_id
                LEFT JOIN programs.classes c ON tac.class_id = c.id
                LEFT JOIN programs.table_programs p ON c.program_id = p.id
                LEFT JOIN programs.table_departments d ON p.department_id = d.id
                WHERE ur.role_name IN ('Teacher', 'Lecturer') AND u.id = @teacherId
                GROUP BY u.id, u.full_name, u.email, u.phone_number, u.created_at, u.account_status, u.avatar_url, u.address, d.name
            """;

            var performanceSql = """
                WITH teacher_stats AS (
                    SELECT 
                        tac.teacher_id,
                        COUNT(DISTINCT tac.id) as courses_count,
                        COUNT(DISTINCT sp.student_id) as students_count,
                        ROUND(CAST(RANDOM() * 20 + 75 AS numeric), 2) as avg_student_score,
                        ROUND(CAST(RANDOM() * 20 + 80 AS numeric), 2) as satisfaction_rate,
                        FLOOR(RANDOM() * 100 + 50) as total_lessons,
                        FLOOR(RANDOM() * 90 + 40) as completed_lessons,
                        FLOOR(RANDOM() * 50 + 20) as total_assignments,
                        ROUND(CAST(RANDOM() * 20 + 75 AS numeric), 2) as assignment_completion_rate,
                        FLOOR(RANDOM() * 200 + 100) as teaching_hours
                    FROM programs.table_teaching_assign_courses tac
                    LEFT JOIN programs.classes c ON tac.class_id = c.id
                    LEFT JOIN programs.table_student_programs sp ON c.program_id = sp.program_id
                    WHERE tac.teacher_id = @teacherId
                    GROUP BY tac.teacher_id
                )
                SELECT 
                    COALESCE(ts.courses_count, 0) as courses_count,
                    COALESCE(ts.students_count, 0) as students_count,
                    COALESCE(ts.avg_student_score, 0) as average_student_score,
                    COALESCE(ts.satisfaction_rate, 0) as student_satisfaction_rate,
                    COALESCE(ts.total_lessons, 0) as total_lessons,
                    COALESCE(ts.completed_lessons, 0) as completed_lessons,
                    COALESCE(ts.total_assignments, 0) as total_assignments,
                    COALESCE(ts.assignment_completion_rate, 0) as assignment_completion_rate,
                    COALESCE(ts.teaching_hours, 0) as teaching_hours,
                    'Top 10%' as rank
                FROM teacher_stats ts
            """;

            var teachingCoursesSql = """
                SELECT 
                    c.id as course_id,
                    tac.course_class_name as CourseName,
                    c.code as course_code,
                    COUNT(DISTINCT sp.student_id) as StudentsEnrolled,
                    50 as max_students,
                    COALESCE(AVG(scp.progress_percentage), 0) as CompletionRate,
                    ROUND(CAST(70 + (RANDOM() * 30) AS numeric), 2) as AverageScore,
                    tac.status,
                    c.created_at as StartDate
                FROM programs.table_teaching_assign_courses tac
                JOIN programs.table_courses c ON tac.course_id = c.id
                JOIN programs.classes cls ON tac.class_id = cls.id
                LEFT JOIN programs.table_student_programs sp ON cls.program_id = sp.program_id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                    AND tac.id = scp.teaching_assign_course_id
                WHERE tac.teacher_id = @teacherId
                GROUP BY c.id, c.name, c.code, tac.status, c.created_at, tac.course_class_name
                ORDER BY c.created_at DESC
            """;

            var monthlyActivitySql = """
                WITH months AS (
                    SELECT 
                        TO_CHAR(generate_series(CURRENT_DATE - INTERVAL '5 months', CURRENT_DATE, INTERVAL '1 month'), 'MM/YYYY') as month
                ),
                mock_activity AS (
                    SELECT 
                        m.month,
                        FLOOR(RANDOM() * 40 + 20) as teaching_hours,
                        FLOOR(RANDOM() * 30 + 10) as students_graded,
                        FLOOR(RANDOM() * 10 + 3) as assignments_created,
                        ROUND(CAST(RANDOM() * 20 + 75 AS numeric), 2) as average_student_score
                    FROM months m
                )
                SELECT * FROM mock_activity ORDER BY month
            """;

            var studentPerformancesSql = """
                SELECT 
                    u.full_name as StudentName,
                    c.name as CourseName,
                    ROUND(CAST(70 + (RANDOM() * 30) AS numeric), 2) as score,
                    COALESCE(scp.progress_percentage, 0) as progress,
                    CASE 
                        WHEN scp.progress_percentage >= 100 THEN 'Completed'
                        WHEN scp.progress_percentage > 0 THEN 'In Progress'
                        ELSE 'Not Started'
                    END as status
                FROM programs.table_teaching_assign_courses tac
                JOIN programs.table_courses c ON tac.course_id = c.id
                JOIN programs.classes cls ON tac.class_id = cls.id
                JOIN programs.table_student_programs sp ON cls.program_id = sp.program_id
                JOIN users.table_users u ON sp.student_id = u.id
                LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id
                    AND tac.id = scp.teaching_assign_course_id
                WHERE tac.teacher_id = @teacherId
                ORDER BY u.full_name
                LIMIT 10
            """;

            var departmentComparisonSql = """
                SELECT 
                    'Student Satisfaction' as metric, 
                    ROUND(CAST(RANDOM() * 20 + 80 AS numeric), 2) as teacher_value,
                    ROUND(CAST(RANDOM() * 15 + 75 AS numeric), 2) as department_average,
                    'up' as trend
                UNION ALL
                SELECT 
                    'Average Score' as metric,
                    ROUND(CAST(RANDOM() * 15 + 80 AS numeric), 2) as teacher_value,
                    ROUND(CAST(RANDOM() * 15 + 75 AS numeric), 2) as department_average,
                    'up' as trend
                UNION ALL
                SELECT 
                    'Course Completion' as metric,
                    ROUND(CAST(RANDOM() * 20 + 75 AS numeric), 2) as teacher_value,
                    ROUND(CAST(RANDOM() * 15 + 70 AS numeric), 2) as department_average,
                    'stable' as trend
                UNION ALL
                SELECT 
                    'Teaching Hours' as metric,
                    FLOOR(RANDOM() * 50 + 150) as teacher_value,
                    FLOOR(RANDOM() * 30 + 120) as department_average,
                    'up' as trend
            """;

            var parameters = new { teacherId = Guid.Parse(request.TeacherId) };

            // Lấy dữ liệu theo model cụ thể
            var teacher = await connection.QueryFirstOrDefaultAsync<TeacherDto>(teacherSql, parameters);
            if (teacher == null)
            {
                return Result.Failure<GetTeacherByIdResponse>(
                    Error.NotFound("Teacher.NotFound", "Teacher not found"));
            }

            var performanceStats = await connection.QueryFirstOrDefaultAsync<PerformanceStatsDto>(performanceSql, parameters);
            var teachingCourses = await connection.QueryAsync<TeachingCourse>(teachingCoursesSql, parameters);
            var monthlyActivities = await connection.QueryAsync<MonthlyTeachingActivity>(monthlyActivitySql, parameters);
            var studentPerformances = await connection.QueryAsync<StudentPerformanceByTeacher>(studentPerformancesSql, parameters);
            var departmentComparisons = await connection.QueryAsync<DepartmentComparison>(departmentComparisonSql, parameters);

            var response = new GetTeacherByIdResponse
            {
                Id = teacher.Id.ToString(),
                Name = teacher.Name ?? string.Empty,
                Email = teacher.Email ?? string.Empty,
                PhoneNumber = teacher.PhoneNumber ?? string.Empty,
                Department = teacher.Department ?? string.Empty,
                JoinDate = teacher.JoinDate?.ToString("yyyy-MM-dd") ?? string.Empty,
                Status = teacher.Status ?? string.Empty,
                Avatar = teacher.Avatar ?? string.Empty,
                Address = teacher.Address ?? string.Empty,
                Specialization = teacher.Specialization ?? string.Empty,
                Rating = teacher.Rating,
                CoursesCount = performanceStats?.CoursesCount ?? 0,
                StudentsCount = performanceStats?.StudentsCount ?? 0,
                PerformanceStats = new TeacherPerformanceStats
                {
                    AverageStudentScore = performanceStats?.AverageStudentScore ?? 0,
                    StudentSatisfactionRate = performanceStats?.StudentSatisfactionRate ?? 0,
                    TotalLessons = performanceStats?.TotalLessons ?? 0,
                    CompletedLessons = performanceStats?.CompletedLessons ?? 0,
                    TotalAssignments = performanceStats?.TotalAssignments ?? 0,
                    AssignmentCompletionRate = performanceStats?.AssignmentCompletionRate ?? 0,
                    TeachingHours = performanceStats?.TeachingHours ?? 0,
                    Rank = performanceStats?.Rank ?? string.Empty
                },
                TeachingCourses = teachingCourses.ToList(),
                TeachingActivities = monthlyActivities.ToList(),
                StudentPerformances = studentPerformances.ToList(),
                DepartmentComparisons = departmentComparisons.ToList()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result.Failure<GetTeacherByIdResponse>(
                Error.Failure("GetTeacherById.Error", ex.Message));
        }
    }

    // DTO cho dữ liệu teacher
    private sealed class TeacherDto
    {
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
        public DateTime? JoinDate { get; init; }
        public string? Status { get; init; }
        public string? Avatar { get; init; }
        public string? Address { get; init; }
        public string? Department { get; init; }
        public string? Specialization { get; init; }
        public decimal Rating { get; init; }
    }

    // DTO cho performance stats
    private sealed class PerformanceStatsDto
    {
        public int CoursesCount { get; init; }
        public int StudentsCount { get; init; }
        public decimal AverageStudentScore { get; init; }
        public decimal StudentSatisfactionRate { get; init; }
        public int TotalLessons { get; init; }
        public int CompletedLessons { get; init; }
        public int TotalAssignments { get; init; }
        public double AssignmentCompletionRate { get; init; }
        public int TeachingHours { get; init; }
        public string? Rank { get; init; }
    }
}
