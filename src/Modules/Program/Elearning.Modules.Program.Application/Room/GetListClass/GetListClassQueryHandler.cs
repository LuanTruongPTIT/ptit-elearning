using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data.Common;

namespace Elearning.Modules.Program.Application.Room.GetListClass;

internal sealed class GetListClassQueryHandler : IQueryHandler<GetListClassQuery, List<GetListClassResponse>>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetListClassQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<List<GetListClassResponse>>> Handle(GetListClassQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    const string sql = """
             SELECT 
                tc.id,
                tc.class_name,
                tc.department_id,
                td.name as department_name,
                tc.program_id,
                tp.name as program_name,
                tc.academic_period,
                tc.status,
                tc.created_at
            FROM programs.classes tc
            INNER JOIN programs.table_departments td ON tc.department_id = td.id
            INNER JOIN programs.table_programs tp ON tc.program_id = tp.id
            ORDER BY tc.created_at DESC
            LIMIT @pageSize OFFSET @offset
            """;

    var result = await connection.QueryAsync<GetListClassResponse>(
        sql,
        new
        {
          pageSize = request.page_size ?? 10,
          offset = ((request.page ?? 1) - 1) * (request.page_size ?? 10)
        }
    );

    return result.ToList();
  }
}