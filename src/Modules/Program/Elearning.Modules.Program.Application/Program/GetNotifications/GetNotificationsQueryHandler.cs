using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetNotifications;

internal sealed class GetNotificationsQueryHandler : IQueryHandler<GetNotificationsQuery, GetNotificationsResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetNotificationsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<GetNotificationsResponse>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

        // Build WHERE clause
        var whereConditions = new List<string>();
        var parameters = new DynamicParameters();

        if (request.user_id.HasValue)
        {
            whereConditions.Add("n.user_id = @user_id");
            parameters.Add("user_id", request.user_id.Value);
        }

        if (request.is_read.HasValue)
        {
            whereConditions.Add("n.is_read = @is_read");
            parameters.Add("is_read", request.is_read.Value);
        }

        var whereClause = whereConditions.Count > 0 ? "WHERE " + string.Join(" AND ", whereConditions) : "";

        // Get total count
        var countSql = $@"
            SELECT COUNT(*) 
            FROM programs.table_notifications n
            {whereClause}";

        var totalCount = await connection.QuerySingleAsync<int>(countSql, parameters);

        // Get unread count for the user
        var unreadCountSql = @"
            SELECT COUNT(*) 
            FROM programs.table_notifications 
            WHERE user_id = @user_id AND is_read = false";

        var unreadCount = request.user_id.HasValue 
            ? await connection.QuerySingleAsync<int>(unreadCountSql, new { user_id = request.user_id.Value })
            : 0;

        // Get notifications with pagination
        parameters.Add("limit", request.limit);
        parameters.Add("offset", request.offset);

        var notificationsSql = $@"
            SELECT 
                n.id,
                n.user_id,
                n.title,
                n.message,
                n.type,
                n.target_type,
                n.target_id,
                n.is_read,
                n.priority,
                n.created_at,
                n.read_at
            FROM programs.table_notifications n
            {whereClause}
            ORDER BY n.created_at DESC
            LIMIT @limit OFFSET @offset";

        var notifications = await connection.QueryAsync(notificationsSql, parameters);

        var notificationDtos = notifications.Select(n => new NotificationDto(
            (Guid)n.id,
            (Guid)n.user_id,
            (string)n.title,
            (string)n.message,
            (string)n.type,
            (string?)n.target_type,
            (Guid?)n.target_id,
            (bool)n.is_read,
            (string)n.priority,
            (DateTime)n.created_at,
            (DateTime?)n.read_at,
            GetTimeAgo((DateTime)n.created_at)
        )).ToList();

        var hasMore = request.offset + request.limit < totalCount;

        return new GetNotificationsResponse(notificationDtos, totalCount, unreadCount, hasMore);
    }

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;

        if (timeSpan.TotalMinutes < 1)
            return "Vừa xong";
        if (timeSpan.TotalMinutes < 60)
            return $"{(int)timeSpan.TotalMinutes} phút trước";
        if (timeSpan.TotalHours < 24)
            return $"{(int)timeSpan.TotalHours} giờ trước";
        if (timeSpan.TotalDays < 7)
            return $"{(int)timeSpan.TotalDays} ngày trước";
        if (timeSpan.TotalDays < 30)
            return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
        if (timeSpan.TotalDays < 365)
            return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";
        
        return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
    }
}
