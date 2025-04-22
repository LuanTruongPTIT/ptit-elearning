using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetProgramByDeparment;

internal sealed class GetProgramByDeparmentQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetProgramByDeparmentQuery, List<GetProgramByDeparmentResponse>>
{
  public async Task<Result<List<GetProgramByDeparmentResponse>>> Handle(GetProgramByDeparmentQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
    const string sql =
      """
      SELECT 
        tp.id,
        tp.name,
        tp.code,
        tp.department_id
      FROM programs.table_programs tp
      WHERE tp.department_id = @department_id
      """;
    List<GetProgramByDeparmentResponse> result = (await connection.QueryAsync<GetProgramByDeparmentResponse>(sql, new { department_id = request.department_id })).AsList();
    return result;
  }
}