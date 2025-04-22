using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using MediatR;

namespace Elearning.Modules.Program.Application.Room;

internal sealed class CreateClassCommandHandler(IDbConnectionFactory dbConnectionFactory) : ICommandHandler<CreateClassCommand, string>
{

  public async Task<Result<string>> Handle(CreateClassCommand request, CancellationToken cancellationToken)
  {

    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
    await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

    try
    {
      var isExist = await findClassNameIsExist(request.className, connection, transaction);
      if (isExist)
      {
        return Result.Failure<string>(Error.Validation("Class", "Class name is already exist!"));
      }
      var classId = await createClass(request, connection, transaction);
      await transaction.CommitAsync(cancellationToken);
      return Result.Success(classId);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      await transaction.RollbackAsync(cancellationToken);
      return Result.Failure<string>(Error.Failure("CreateClass", $"An error occurred: {ex.Message}"));
    }
  }
  private async Task<bool> findClassNameIsExist(string className, DbConnection connection, DbTransaction transaction)
  {
    const string sql = "SELECT exists(select * FROM programs.classes WHERE class_name = @class_name)";
    return await connection.ExecuteScalarAsync<bool>(sql, new { class_name = className }, transaction);
  }

  private async Task<string> createClass(CreateClassCommand request, DbConnection connection, DbTransaction transaction)
  {
    const string sql =
      """
      INSERT INTO programs.classes (id, class_name, department_id, program_id, academic_period, status)
      VALUES (@id, @class_name, @department_id, @program_id, @academic_period, @status)
      RETURNING id
      """;
    var result = await connection.ExecuteScalarAsync<Guid>(sql, new
    {
      id = Guid.NewGuid(),
      class_name = request.className,
      request.department_id,
      request.program_id,
      academic_period = request.academicPeriod,
      request.status
    }, transaction);
    return result.ToString();
  }
}