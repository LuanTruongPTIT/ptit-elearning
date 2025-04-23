using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data;

namespace Elearning.Modules.Program.Application.Program.UpdateCourseMaterialPublishStatus;

internal sealed class UpdateCourseMaterialPublishStatusCommandHandler : ICommandHandler<UpdateCourseMaterialPublishStatusCommand, bool>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public UpdateCourseMaterialPublishStatusCommandHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<bool>> Handle(UpdateCourseMaterialPublishStatusCommand request, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            UPDATE programs.table_course_materials
            SET
                is_published = @is_published,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = @id
            RETURNING id;
        """;

    try
    {
      var result = await connection.ExecuteScalarAsync<Guid?>(
          sql,
          new
          {
            request.id,
            request.is_published
          }
      );

      if (result == null)
      {
        return Result.Failure<bool>(new Error(
            "UpdateCourseMaterialPublishStatus.NotFound",
            "Course material not found",
            ErrorType.NotFound));
      }

      return Result.Success(true);
    }
    catch (Exception ex)
    {
      return Result.Failure<bool>(new Error(
          "UpdateCourseMaterialPublishStatus.Error",
          $"Failed to update course material publish status: {ex.Message}",
          ErrorType.Failure));
    }
  }
}
