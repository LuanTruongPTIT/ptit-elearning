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
        const string notificationsSql = @"
                SELECT 
                    n.id,
                    n.title,
                    n.message,
                    n.type,
                    n.target_type,
                    n.target_id,
                    n.is_read,
                    n.created_at
                FROM programs.table_notifications n
                WHERE n.user_id = @studentId
                    AND (@notificationType IS NULL OR n.type = @notificationType)
                ORDER BY n.created_at DESC
                LIMIT @pageSize OFFSET @offset";

        var offset = (request.PageNumber - 1) * request.PageSize;
        var notifications = await connection.QueryAsync<dynamic>(notificationsSql, new
        {
            studentId = request.StudentId,
            notificationType = request.NotificationType,
            pageSize = request.PageSize,
            offset = offset
        });

        var notificationDtos = notifications.Select(n =>
        {
            var createdAt = (DateTime)n.created_at;

            return new StudentNotificationDto
            {
                Id = n.id.ToString(),
                Type = n.type?.ToString() ?? "",
                Title = n.title?.ToString() ?? "",
                Message = n.message?.ToString() ?? "",
                AssignmentId = n.target_type?.ToString() == "assignment" ? n.target_id?.ToString() : null,
                CourseId = null, // Will be populated later if needed
                Deadline = null, // Not stored in this table
                AssignmentType = n.target_type?.ToString(),
                CreatedAt = createdAt,
                IsRead = (bool)(n.is_read ?? false),
                IsNew = createdAt > DateTime.UtcNow.AddHours(-24)
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
