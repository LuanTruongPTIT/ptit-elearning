using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetStudentNotifications;

public class GetStudentNotificationsQueryHandler : IRequestHandler<GetStudentNotificationsQuery, GetStudentNotificationsResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetStudentNotificationsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<GetStudentNotificationsResponse> Handle(GetStudentNotificationsQuery request, CancellationToken cancellationToken)
    {

        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
        Console.WriteLine(request.StudentId);
        // Get total count of notifications for the student
        const string countSql = @"
                SELECT COUNT(*)
                FROM programs.table_notifications n
                WHERE n.user_id = @studentId
                    AND (@notificationType IS NULL OR n.type = @notificationType)";

        var totalCount = await connection.QuerySingleAsync<int>(countSql, new
        {
            studentId = request.StudentId,
            notificationType = request.NotificationType
        });

        // Get notifications with pagination
        const string sql = @"
                SELECT 
                    n.id,
                    n.type,
                    n.title,
                    n.message,
                    n.target_id,
                    n.target_type,
                    n.is_read,
                    n.created_at,
                    CASE WHEN n.created_at > @recentThreshold THEN true ELSE false END as is_new,
                    a.deadline,
                    a.assignment_type,
                    tac.course_class_name as course_name
                FROM programs.table_notifications n
                LEFT JOIN programs.table_assignments a ON n.target_id = a.id AND n.target_type = 'assignment'
                LEFT JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                WHERE n.user_id = @StudentId
                    AND (@NotificationType IS NULL OR n.type = @NotificationType)
                ORDER BY n.created_at DESC
                LIMIT @PageSize OFFSET @Offset";

        var offset = (request.PageNumber - 1) * request.PageSize;
        var notifications = await connection.QueryAsync<dynamic>(sql, new
        {
            StudentId = request.StudentId,
            NotificationType = request.NotificationType,
            PageSize = request.PageSize,
            Offset = offset,
            recentThreshold = DateTime.UtcNow.AddHours(-24)
        });
        Console.WriteLine("notifications", notifications.ToString());

        var notificationDtos = notifications.Select(n =>
        {
            var createdAt = (DateTime)n.created_at;
            Console.WriteLine("assignment", n.target_id?.ToString());
            return new StudentNotificationDto
            {
                Id = n.id.ToString(),
                Type = n.type?.ToString() ?? "",
                Title = n.title?.ToString() ?? "",
                Message = n.message?.ToString() ?? "",
                AssignmentId = n.target_type?.ToString() == "assignment" ? n.target_id?.ToString() : null,
                CourseId = null, // Will be populated later if needed
                CourseName = n.course_name?.ToString(),
                Deadline = n.deadline != null ? (DateTime?)n.deadline : null,
                AssignmentType = n.assignment_type?.ToString(),
                CreatedAt = createdAt,
                IsRead = (bool)(n.is_read ?? false),
                IsNew = (bool)(n.is_new ?? false)
            };
        }).ToList();

        return new GetStudentNotificationsResponse
        {
            Notifications = notificationDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            HasNextPage = (request.PageNumber * request.PageSize) < totalCount
        };
    }
}
