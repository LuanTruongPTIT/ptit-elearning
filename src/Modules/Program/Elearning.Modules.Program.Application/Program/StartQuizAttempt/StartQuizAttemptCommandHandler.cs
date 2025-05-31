using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Modules.Program.Domain.Program;

namespace Elearning.Modules.Program.Application.Program.StartQuizAttempt;

internal sealed class StartQuizAttemptCommandHandler : ICommandHandler<StartQuizAttemptCommand, Guid>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public StartQuizAttemptCommandHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<Guid>> Handle(StartQuizAttemptCommand request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

        // Check if quiz exists
        const string checkQuizSql = @"
            SELECT quiz_id, max_attempts
            FROM programs.table_quizzes
            WHERE quiz_id = @quiz_id";

        var quiz = await connection.QueryFirstOrDefaultAsync(checkQuizSql, new { quiz_id = request.quiz_id });

        if (quiz == null)
        {
            return Result.Failure<Guid>(Error.Failure("Quiz.NotFound", "Quiz not found"));
        }

        // Check current attempt count
        const string attemptCountSql = @"
            SELECT COUNT(*)
            FROM programs.table_quiz_attempts
            WHERE quiz_id = @quiz_id AND student_id = @student_id";

        var currentAttempts = await connection.QueryFirstOrDefaultAsync<int>(
            attemptCountSql,
            new { quiz_id = request.quiz_id, student_id = request.student_id });

        if (currentAttempts >= quiz.max_attempts)
        {
            return Result.Failure<Guid>(Error.Failure("Quiz.MaxAttemptsReached", "Maximum attempts reached for this quiz"));
        }

        // Check if there's an in-progress attempt
        const string inProgressSql = @"
            SELECT attempt_id
            FROM programs.table_quiz_attempts
            WHERE quiz_id = @quiz_id AND student_id = @student_id AND status = 'in_progress'";

        var existingAttempt = await connection.QueryFirstOrDefaultAsync<Guid?>(
            inProgressSql,
            new { quiz_id = request.quiz_id, student_id = request.student_id });

        if (existingAttempt.HasValue)
        {
            return Result.Failure<Guid>(Error.Failure("Quiz.AttemptInProgress", "There is already an attempt in progress"));
        }

        // Create new attempt
        var attemptId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var attempt = new QuizAttempt
        {
            attempt_id = attemptId,
            quiz_id = request.quiz_id,
            student_id = request.student_id,
            attempt_number = currentAttempts + 1,
            status = "in_progress",
            started_at = now,
            ip_address = request.ip_address,
            user_agent = request.user_agent,
            created_at = now,
            updated_at = now,
            created_by = request.student_id
        };

        const string insertAttemptSql = @"
            INSERT INTO programs.table_quiz_attempts (
                attempt_id, quiz_id, student_id, attempt_number, status,
                started_at, ip_address, user_agent, created_at, updated_at, created_by
            ) VALUES (
                @attempt_id, @quiz_id, @student_id, @attempt_number, @status,
                @started_at, @ip_address, @user_agent, @created_at, @updated_at, @created_by
            )";

        await connection.ExecuteAsync(insertAttemptSql, attempt);

        return attemptId;
    }
}
