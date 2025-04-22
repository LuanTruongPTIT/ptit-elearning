using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetDepartment;


public class GetDepartmentQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetDepartmentQuery, List<GetDepartmentResponse>>
{
  public async Task<Result<List<GetDepartmentResponse>>> Handle(GetDepartmentQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
    string sql =
      """
      SELECT
        tp.id,
        tp.name,
        tp.code
      FROM programs.table_departments tp
   """;
    List<GetDepartmentResponse> result = (await connection.QueryAsync<GetDepartmentResponse>(sql)).AsList();
    return result;
  }
}
