using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Dapper;
using System.Data.Common;
using Serilog.Core;
namespace Elearning.Modules.Program.Application.Program;

internal sealed class ProgramGetCourseDepartmentQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<ProgramGetCourseDepartmentQuery, List<ProgramGetCourseDepartmentResponse>>
{
  public async Task<Result<List<ProgramGetCourseDepartmentResponse>>> Handle(ProgramGetCourseDepartmentQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
    const string sql =
     $""" 
     SELECT DISTINCT ON (tc.id)
     tc.id AS course_id,
     tc.name AS course_name,
     td.id AS department_id,
     td.name AS department_name
        FROM programs.table_departments td
        LEFT JOIN programs.table_programs tp ON td.id = tp.department_id
        INNER JOIN programs.table_program_courses tpc ON tp.id = tpc.program_id
        INNER JOIN programs.table_courses tc ON tc.id = tpc.course_id;
     """;
    List<ProgramGetCourseDepartmentResponse> result = (await connection.QueryAsync<ProgramGetCourseDepartmentResponse>(sql)).AsList();
    // Logger.Information("Get course department successfully", result);
    return result;
  }
}