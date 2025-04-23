using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetProgram;

internal sealed class GetProgramQueryHandler(IDbConnectionFactory dbConnectionFactory) 
    : IQueryHandler<GetProgramQuery, GetProgramResponse>
{
    public async Task<Result<GetProgramResponse>> Handle(GetProgramQuery request, CancellationToken cancellationToken)
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
            WHERE 
                p.id = @programId
            """;
        
        try
        {
            var program = await connection.QueryFirstOrDefaultAsync<GetProgramResponse>(
                sql, 
                new { programId = Guid.Parse(request.programId) }
            );
            
            if (program == null)
            {
                return Result.Failure<GetProgramResponse>(
                    Error.NotFound("GetProgram.NotFound", $"Program with ID {request.programId} not found"));
            }
            
            return Result.Success(program);
        }
        catch (Exception ex)
        {
            return Result.Failure<GetProgramResponse>(
                Error.Failure("GetProgram.Query", $"Failed to retrieve program: {ex.Message}"));
        }
    }
}
