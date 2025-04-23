using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data.Common;

namespace Elearning.Modules.Program.Application.Program.GetLectures;

internal sealed class GetLecturesQueryHandler : IQueryHandler<GetLecturesQuery, List<GetLecturesResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetLecturesQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<List<GetLecturesResponse>>> Handle(GetLecturesQuery request, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            SELECT
                id,
                course_id,
                teaching_assign_course_id,
                title,
                description,
                content_type,
                content_url,
                youtube_video_id,
                duration,
                is_published,
                created_at,
                updated_at,
                created_by
            FROM programs.table_lectures
            WHERE teaching_assign_course_id = @teaching_assign_course_id
            ORDER BY created_at DESC;
        """;

    try
    {
      var result = await connection.QueryAsync<GetLecturesResponse>(
          sql,
          new { request.teaching_assign_course_id }
      );

      return Result.Success(result.ToList());
    }
    catch (Exception ex)
    {
      return Result.Failure<List<GetLecturesResponse>>(new Error(
          "GetLectures.Error",
          $"Failed to get lectures: {ex.Message}",
          ErrorType.Failure));
    }
  }
}
