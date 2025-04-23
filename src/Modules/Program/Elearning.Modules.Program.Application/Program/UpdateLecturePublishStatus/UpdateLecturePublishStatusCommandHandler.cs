using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data.Common;

namespace Elearning.Modules.Program.Application.Program.UpdateLecturePublishStatus;

internal sealed class UpdateLecturePublishStatusCommandHandler : ICommandHandler<UpdateLecturePublishStatusCommand, bool>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public UpdateLecturePublishStatusCommandHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<bool>> Handle(UpdateLecturePublishStatusCommand request, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.OpenConnectionAsync();

        const string sql = """
            UPDATE programs.table_lectures
            SET 
                is_published = @is_published,
                updated_at = CURRENT_TIMESTAMP
            WHERE id = @id
            RETURNING id;
        """;

        try
        {
            var result = await connection.ExecuteScalarAsync<Guid?>(
                sql,
                new { 
                    request.id,
                    request.is_published
                }
            );

            if (result == null)
            {
                return Result.Failure<bool>(new Error(
                    "UpdateLecturePublishStatus.NotFound",
                    "Lecture not found",
                    ErrorType.NotFound));
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new Error(
                "UpdateLecturePublishStatus.Error",
                $"Failed to update lecture publish status: {ex.Message}",
                ErrorType.Failure));
        }
    }
}
