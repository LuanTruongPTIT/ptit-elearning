using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetClassByDepartmentOfTeacher;

public class GetClassByDepartmentOfTeacherQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetClassByDepartmentOfTeacherQuery, List<GetClassByDepartmentOfTeacherResponse>>
{
  public async Task<Result<List<GetClassByDepartmentOfTeacherResponse>>> Handle(GetClassByDepartmentOfTeacherQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
    await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);
    try
    {
      var result = await GetClassByDepartmentOfTeacher(request, connection, transaction);
      await transaction.CommitAsync(cancellationToken);
      return Result.Success(result);
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      await transaction.RollbackAsync(cancellationToken);
      return Result.Failure<List<GetClassByDepartmentOfTeacherResponse>>(Error.Failure("GetClassByDepartmentOfTeacher", $"An error occurred: {ex.Message}"));
    }
  }

  private async Task<List<GetClassByDepartmentOfTeacherResponse>> GetClassByDepartmentOfTeacher(GetClassByDepartmentOfTeacherQuery request, DbConnection connection, DbTransaction transaction)
  {
    string sql = """
        SELECT
              pc.id AS class_id,
              pc.class_name,
              pc.department_id,
              pc.program_id,
              tp.name AS program_name,
              ta.teacher_id
          FROM
              programs.classes pc
              INNER JOIN programs.table_programs tp 
              ON pc.program_id = tp.id
              INNER JOIN programs.table_departments td 
              ON pc.department_id = td.id
              INNER JOIN programs.table_teaching_assignments ta 
              ON ta.department_id = pc.department_id
             WHERE
             ta.teacher_id = @teacher_id;
        """;
    var result = await connection.QueryAsync<GetClassByDepartmentOfTeacherResponse>(sql, new { teacher_id = Guid.Parse(request.teacher_id) }, transaction);
    return result.ToList();
  }
}