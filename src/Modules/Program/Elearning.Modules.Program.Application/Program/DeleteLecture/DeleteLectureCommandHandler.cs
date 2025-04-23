using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Data.Common;

namespace Elearning.Modules.Program.Application.Program.DeleteLecture;

internal sealed class DeleteLectureCommandHandler : ICommandHandler<DeleteLectureCommand, bool>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public DeleteLectureCommandHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<bool>> Handle(DeleteLectureCommand request, CancellationToken cancellationToken)
    {
        using var connection = await _dbConnectionFactory.OpenConnectionAsync();

        const string sql = """
            DELETE FROM programs.table_lectures
            WHERE id = @id
            RETURNING id;
        """;

        try
        {
            var result = await connection.ExecuteScalarAsync<Guid?>(
                sql,
                new { request.id }
            );

            if (result == null)
            {
                return Result.Failure<bool>(new Error(
                    "DeleteLecture.NotFound",
                    "Lecture not found",
                    ErrorType.NotFound));
            }

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(new Error(
                "DeleteLecture.Error",
                $"Failed to delete lecture: {ex.Message}",
                ErrorType.Failure));
        }
    }
}
