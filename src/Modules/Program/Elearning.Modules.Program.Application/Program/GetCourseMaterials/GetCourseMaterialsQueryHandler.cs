using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data;

namespace Elearning.Modules.Program.Application.Program.GetCourseMaterials;

internal sealed class GetCourseMaterialsQueryHandler : IQueryHandler<GetCourseMaterialsQuery, List<GetCourseMaterialsResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetCourseMaterialsQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<List<GetCourseMaterialsResponse>>> Handle(GetCourseMaterialsQuery request, CancellationToken cancellationToken)
  {
    using IDbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            SELECT
                id,
                course_id,
                title,
                description,
                file_url,
                file_type,
                file_size,
                is_published,
                created_at,
                updated_at,
                created_by,
                youtube_video_id,
                content_type
            FROM programs.table_course_materials
            WHERE course_id = @course_id
            ORDER BY created_at DESC;
        """;

    try
    {
      var result = await connection.QueryAsync<GetCourseMaterialsResponse>(
          sql,
          new { request.course_id }
      );

      return Result.Success(result.ToList());
    }
    catch (Exception ex)
    {
      return Result.Failure<List<GetCourseMaterialsResponse>>(new Error(
          "GetCourseMaterials.Error",
          $"Failed to get course materials: {ex.Message}",
          ErrorType.Failure));
    }
  }
}
