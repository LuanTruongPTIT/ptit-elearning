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
    try
    {
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

      // Build WHERE clause
      var whereClause = request.user_id.HasValue ? "WHERE user_id = @user_id" : "";

      // Create a UNION query to get activities from multiple sources
      var activitiesSql = $@"
            WITH recent_activities AS (
                -- Assignment submissions
                SELECT 
                    asub.id,
                    asub.student_id as user_id,
                    'assignment_submitted' as action,
                    'assignment' as target_type,
                    asub.assignment_id as target_id,
                    a.title as target_title,
                    tac.id as course_id,
                    tac.course_class_name,
                    json_build_object(
                        'grade', asub.grade,
                        'status', asub.status,
                        'submission_type', asub.submission_type
                    ) as metadata,
                    asub.submitted_at as created_at
                FROM programs.table_assignment_submissions asub
                INNER JOIN programs.table_assignments a ON asub.assignment_id = a.id
                INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                
                UNION ALL
                
                -- Course enrollments (when student enrolls in a course)
                SELECT 
                    se.id,
                    se.student_id as user_id,
                    'course_enrolled' as action,
                    'course' as target_type,
                    se.teaching_assign_course_id as target_id,
                    tac.course_class_name as target_title,
                    tac.id as course_id,
                    tac.course_class_name,
                    json_build_object(
                        'status', se.status,
                        'enrollment_date', se.enrollment_date
                    ) as metadata,
                    se.enrollment_date as created_at
                FROM programs.table_student_enrollments se
                INNER JOIN programs.table_teaching_assign_courses tac ON se.teaching_assign_course_id = tac.id
                
                UNION ALL
                
                -- Course progress updates (when student makes progress)
                SELECT 
                    scp.id,
                    scp.student_id as user_id,
                    CASE 
                        WHEN scp.progress_percentage = 100 THEN 'course_completed'
                        ELSE 'course_progress_updated'
                    END as action,
                    'course' as target_type,
                    scp.teaching_assign_course_id as target_id,
                    tac.course_class_name as target_title,
                    tac.id as course_id,
                    tac.course_class_name,
                    json_build_object(
                        'progress_percentage', scp.progress_percentage,
                        'completed_lectures', scp.completed_lectures,
                        'total_lectures', scp.total_lectures,
                        'status', scp.status
                    ) as metadata,
                    scp.updated_at as created_at
                FROM programs.table_student_course_progress scp
                INNER JOIN programs.table_teaching_assign_courses tac ON scp.teaching_assign_course_id = tac.id
                WHERE scp.updated_at > scp.created_at -- Only show actual updates, not initial creation
            )
            SELECT
                ra.id,
                ra.user_id,
                ra.action,
                ra.target_type,
                ra.target_id,
                ra.target_title,
                ra.course_id,
                ra.course_class_name,
                ra.metadata,
                ra.created_at
            FROM recent_activities ra
            {whereClause}
            ORDER BY ra.created_at DESC
            LIMIT @limit OFFSET @offset";

      var activities = await connection.QueryAsync(activitiesSql, new
      {
        user_id = request.user_id,
        limit = request.limit,
        offset = request.offset
      });

      // Get total count
      var countSql = $@"
            WITH recent_activities AS (
                -- Same UNION query as above but just for counting
                SELECT asub.student_id as user_id, asub.submitted_at as created_at
                FROM programs.table_assignment_submissions asub
                UNION ALL
                SELECT se.student_id as user_id, se.enrollment_date as created_at
                FROM programs.table_student_enrollments se
                UNION ALL
                SELECT scp.student_id as user_id, scp.updated_at as created_at
                FROM programs.table_student_course_progress scp
                WHERE scp.updated_at > scp.created_at
            )
            SELECT COUNT(*)
            FROM recent_activities ra
            {whereClause}";

      var totalCount = await connection.QuerySingleAsync<int>(countSql, new { user_id = request.user_id });

      var activityDtos = activities.Select(a => new RecentActivityDto(
          (Guid)a.id,
          (Guid)a.user_id,
          (string)a.action,
          (string)a.target_type,
          (Guid)a.target_id,
          (string?)a.target_title,
          (Guid?)a.course_id,
          (string?)a.course_class_name,
          a.metadata != null ? JsonSerializer.Deserialize<object>((string)a.metadata) : null,
          (DateTime)a.created_at,
          GetTimeAgo((DateTime)a.created_at)
      )).ToList();

      var hasMore = request.offset + request.limit < totalCount;

      return new GetRecentActivitiesResponse(activityDtos, totalCount, hasMore);
    }
    catch (Exception ex)
    {
      // Log error and return empty response
      Console.WriteLine($"Error in GetRecentActivitiesQueryHandler: {ex.Message}");

      return new GetRecentActivitiesResponse(new List<RecentActivityDto>(), 0, false);
    }
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
