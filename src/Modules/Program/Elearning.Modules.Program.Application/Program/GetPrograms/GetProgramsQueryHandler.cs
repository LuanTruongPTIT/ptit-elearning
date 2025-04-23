using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetPrograms;

internal sealed class GetProgramsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetProgramsQuery, List<GetProgramsResponse>>
{
  public async Task<Result<List<GetProgramsResponse>>> Handle(GetProgramsQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
            SELECT 
                p.id,
                p.name,
                p.code,
                p.department_id,
                d.name as department_name,
                p.created_at,
                p.updated_at
            FROM 
                programs.table_programs p
            JOIN 
                programs.table_departments d ON p.department_id = d.id
            ORDER BY 
                p.name
            """;

    try
    {
      var programs = await connection.QueryAsync<GetProgramsResponse>(sql);
      return Result.Success(programs.ToList());
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      return Result.Failure<List<GetProgramsResponse>>(
    Error.Failure("GetPrograms.Query", $"Failed to retrieve programs: {ex.Message}"));
    }
  }
}
