using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data;

namespace Elearning.Modules.Program.Application.Program.CreateCourseMaterial;

internal sealed class CreateCourseMaterialCommandHandler : ICommandHandler<CreateCourseMaterialCommand, Guid>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public CreateCourseMaterialCommandHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<Guid>> Handle(CreateCourseMaterialCommand request, CancellationToken cancellationToken)
  {
    using IDbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            INSERT INTO programs.table_course_materials (
                id,
                course_id,
                title,
                description,
                file_url,
                file_type,
                file_size,
                is_published,
                created_by,
                content_type
            ) VALUES (
                @id,
                @course_id,
                @title,
                @description,
                @file_url,
                @file_type,
                @file_size,
                @is_published,
                @created_by,
                @content_type
            )
            RETURNING id;
        """;

    try
    {
      var id = Guid.NewGuid();

      await connection.ExecuteAsync(
          sql,
          new
          {
            id,
            request.course_id,
            request.title,
            request.description,
            request.file_url,
            request.file_type,
            request.file_size,
            request.is_published,
            request.created_by,
            request.content_type
          }
      );

      return Result.Success(id);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<Guid>(new Error(
          "CreateCourseMaterial.Error",
          $"Failed to create course material: {ex.Message}",
          ErrorType.Failure));
    }
  }
}
