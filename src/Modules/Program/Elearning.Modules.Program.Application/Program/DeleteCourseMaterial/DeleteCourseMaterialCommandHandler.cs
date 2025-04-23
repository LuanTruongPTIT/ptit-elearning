using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data;

namespace Elearning.Modules.Program.Application.Program.DeleteCourseMaterial;

internal sealed class DeleteCourseMaterialCommandHandler : ICommandHandler<DeleteCourseMaterialCommand, bool>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public DeleteCourseMaterialCommandHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<bool>> Handle(DeleteCourseMaterialCommand request, CancellationToken cancellationToken)
  {
    using var connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            DELETE FROM programs.table_course_materials
            WHERE id = @id
            RETURNING id;
        """;

    try
    {
      var result = await connection.ExecuteScalarAsync<Guid?>(
          sql,
          new { request.id }
      );

      if (result == null)
      {
        return Result.Failure<bool>(new Error(
            "DeleteCourseMaterial.NotFound",
            "Course material not found",
            ErrorType.NotFound));
      }

      return Result.Success(true);
    }
    catch (Exception ex)
    {
      return Result.Failure<bool>(new Error(
          "DeleteCourseMaterial.Error",
          $"Failed to delete course material: {ex.Message}",
          ErrorType.Failure));
    }
  }
}
