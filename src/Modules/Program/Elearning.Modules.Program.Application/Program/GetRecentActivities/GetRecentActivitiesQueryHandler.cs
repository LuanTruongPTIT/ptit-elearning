using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetRecentActivities;

internal sealed class GetRecentActivitiesQueryHandler : IQueryHandler<GetRecentActivitiesQuery, GetRecentActivitiesResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetRecentActivitiesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<GetRecentActivitiesResponse>> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    // Build WHERE clause
    var whereClause = request.user_id.HasValue ? "WHERE ra.user_id = @user_id" : "";

    // Get total count
    var countSql = $@"
            SELECT COUNT(*)
            FROM programs.table_recent_activities ra
            {whereClause}";

    var totalCount = await connection.QuerySingleAsync<int>(countSql, new { user_id = request.user_id });

    // Get activities with pagination
    var activitiesSql = $@"
            SELECT
                ra.id,
                ra.user_id,
                ra.action,
                ra.target_type,
                ra.target_id,
                ra.target_title,
                ra.course_id,
                ra.course_name,
                ra.metadata,
                ra.created_at
            FROM programs.table_recent_activities ra
            {whereClause}
            ORDER BY ra.created_at DESC
            LIMIT @limit OFFSET @offset";

    var activities = await connection.QueryAsync(activitiesSql, new
    {
      user_id = request.user_id,
      limit = request.limit,
      offset = request.offset
    });

    var activityDtos = activities.Select(a => new RecentActivityDto(
        (Guid)a.id,
        (Guid)a.user_id,
        (string)a.action,
        (string)a.target_type,
        (Guid)a.target_id,
        (string?)a.target_title,
        (Guid?)a.course_id,
        (string?)a.course_name,
        a.metadata != null ? JsonSerializer.Deserialize<object>((string)a.metadata) : null,
        (DateTime)a.created_at,
        GetTimeAgo((DateTime)a.created_at)
    )).ToList();

    var hasMore = request.offset + request.limit < totalCount;

    return new GetRecentActivitiesResponse(activityDtos, totalCount, hasMore);
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
