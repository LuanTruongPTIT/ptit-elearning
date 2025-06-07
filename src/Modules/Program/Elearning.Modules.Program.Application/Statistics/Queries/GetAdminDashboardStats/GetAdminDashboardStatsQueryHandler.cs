using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Statistics.Queries.GetAdminDashboardStats;

internal sealed class GetAdminDashboardStatsQueryHandler(
    IDbConnectionFactory dbConnectionFactory
) : IQueryHandler<GetAdminDashboardStatsQuery, AdminDashboardStatsResponse>
{
    public async Task<Result<AdminDashboardStatsResponse>> Handle(
        GetAdminDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        try
        {
            // Overview stats
            var overviewSql = """
                SELECT 
                    (SELECT COUNT(*) FROM users.table_users u
                    LEFT JOIN users.table_user_roles ur ON u.id = ur.user_id
                    WHERE ur.role_name = 'Student') as TotalStudents,
                    (SELECT COUNT(*) FROM users.table_users u
                    LEFT JOIN users.table_user_roles ur ON u.id = ur.user_id
                     WHERE ur.role_name IN ('Teacher', 'Lecturer')) as TotalTeachers,
                    (SELECT COUNT(*) FROM programs.table_courses) as TotalCourses,
                    (SELECT COUNT(*) FROM programs.table_teaching_assign_courses WHERE status = 'active') as ActiveCourses,
                    (SELECT COALESCE(AVG(
                        CASE 
                            WHEN scp.total_lectures > 0 
                            THEN (scp.completed_lectures::float / scp.total_lectures::float) * 10 
                            ELSE 0 
                        END
                    ), 0) FROM programs.table_student_course_progress scp) as AverageGpa,
                    (SELECT COALESCE(AVG(scp.progress_percentage), 0) 
                     FROM programs.table_student_course_progress scp 
                     WHERE scp.total_lectures > 0) as CompletionRate
            """;

            // Department stats
            var departmentSql = """
                WITH dept_stats AS (
                    SELECT 
                        d.name as department,
                        COUNT(DISTINCT sp.student_id) as students,
                        COUNT(DISTINCT c.id) as courses,
                        COALESCE(AVG(
                            CASE 
                                WHEN scp.total_lectures > 0 
                                THEN (scp.completed_lectures::float / scp.total_lectures::float) * 10 
                                ELSE 0 
                            END
                        ), 0) as average_gpa
                    FROM programs.table_departments d
                    LEFT JOIN programs.table_programs p ON d.id = p.department_id
                    LEFT JOIN programs.table_student_programs sp ON p.id = sp.program_id
                    LEFT JOIN programs.classes cls ON p.id = cls.program_id
                    LEFT JOIN programs.table_teaching_assign_courses tac ON cls.id = tac.class_id
                    LEFT JOIN programs.table_courses c ON tac.course_id = c.id
                    LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id 
                        AND tac.id = scp.teaching_assign_course_id
                    GROUP BY d.id, d.name
                )
                SELECT * FROM dept_stats
                ORDER BY students DESC
            """;

            // Enrollment trends (last 6 months)
            var enrollmentSql = """
                WITH monthly_data AS (
                    SELECT 
                        TO_CHAR(u.created_at, 'MM/YYYY') as month,
                        COUNT(DISTINCT CASE WHEN ur.role_name = 'Student' THEN u.id END) as students,
                        COUNT(DISTINCT tac.id) as courses
                    FROM users.table_users u
                    LEFT JOIN users.table_user_roles ur ON u.id = ur.user_id
                    FULL OUTER JOIN programs.table_teaching_assign_courses tac 
                        ON DATE_TRUNC('month', u.created_at) = DATE_TRUNC('month', tac.created_at)
                    WHERE u.created_at >= NOW() - INTERVAL '6 months' 
                        OR tac.created_at >= NOW() - INTERVAL '6 months'
                    GROUP BY TO_CHAR(u.created_at, 'MM/YYYY'), TO_CHAR(tac.created_at, 'MM/YYYY')
                )
                SELECT 
                    COALESCE(month, TO_CHAR(NOW() - INTERVAL '6 months', 'MM/YYYY')) as month,
                    COALESCE(students, 0) as students,
                    COALESCE(courses, 0) as courses
                FROM monthly_data
                ORDER BY month
            """;

            // Performance metrics
            var performanceSql = """
                WITH student_gpa AS (
                    SELECT 
                        sp.student_id,
                        COALESCE(AVG(
                            CASE 
                                WHEN scp.total_lectures > 0 
                                THEN (scp.completed_lectures::float / scp.total_lectures::float) * 10 
                                ELSE 0 
                            END
                        ), 0) as gpa
                    FROM programs.table_student_programs sp
                    LEFT JOIN programs.classes cls ON sp.program_id = cls.program_id
                    LEFT JOIN programs.table_teaching_assign_courses tac ON cls.id = tac.class_id
                    LEFT JOIN programs.table_student_course_progress scp ON sp.student_id = scp.student_id 
                        AND tac.id = scp.teaching_assign_course_id
                    GROUP BY sp.student_id
                )
                SELECT 
                    COUNT(CASE WHEN gpa >= 8.5 THEN 1 END) as excellent_students,
                    COUNT(CASE WHEN gpa >= 7.0 AND gpa < 8.5 THEN 1 END) as good_students,
                    COUNT(CASE WHEN gpa >= 5.5 AND gpa < 7.0 THEN 1 END) as average_students,
                    COUNT(CASE WHEN gpa < 5.5 THEN 1 END) as below_average_students
                FROM student_gpa
            """;

            // Recent activities
            var recentActivitiesSql = """
                SELECT 
                    'enrollment' as type,
                    'Sinh viên ' || u.full_name || ' đã đăng ký vào hệ thống' as message,
                    u.created_at::text as timestamp
                FROM users.table_users u 
                LEFT JOIN users.table_user_roles ur ON u.id = ur.user_id
                WHERE ur.role_name = 'Student' AND u.created_at >= NOW() - INTERVAL '7 days'
                
                UNION ALL
                
                SELECT 
                    'course_completion' as type,
                    'Khóa học ' || tac.course_class_name || ' đã được hoàn thành' as message,
                    scp.updated_at::text as timestamp
                FROM programs.table_student_course_progress scp
                JOIN programs.table_teaching_assign_courses tac ON scp.teaching_assign_course_id = tac.id
                WHERE scp.status = 'completed' AND scp.updated_at >= NOW() - INTERVAL '7 days'
                
                ORDER BY timestamp DESC
                LIMIT 10
            """;

            var overview = await connection.QuerySingleAsync<OverviewStats>(overviewSql);
            var departments = await connection.QueryAsync<DepartmentStat>(departmentSql);
            var enrollments = await connection.QueryAsync<EnrollmentTrend>(enrollmentSql);
            var performance = await connection.QuerySingleAsync<PerformanceMetric>(performanceSql);
            var activities = await connection.QueryAsync<RecentActivity>(recentActivitiesSql);

            var response = new AdminDashboardStatsResponse
            {
                Overview = overview,
                DepartmentStats = departments.ToList(),
                EnrollmentTrends = enrollments.ToList(),
                PerformanceMetrics = performance,
                RecentActivities = activities.ToList()
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetAdminDashboardStatsQueryHandler: {ex}");
            return Result.Failure<AdminDashboardStatsResponse>(
                      Error.Failure("GetAdminDashboardStats.Error", ex.Message));
        }
    }
}
