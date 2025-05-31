using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Modules.Program.Domain.Program;

namespace Elearning.Modules.Program.Application.Program.SubmitQuizResponse;

internal sealed class SubmitQuizResponseCommandHandler : ICommandHandler<SubmitQuizResponseCommand, bool>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public SubmitQuizResponseCommandHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<bool>> Handle(SubmitQuizResponseCommand request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

        // Verify attempt exists and is in progress
        const string checkAttemptSql = @"
            SELECT attempt_id, status
            FROM programs.table_quiz_attempts
            WHERE attempt_id = @attempt_id";

        var attempt = await connection.QueryFirstOrDefaultAsync(
            checkAttemptSql,
            new { attempt_id = request.attempt_id });

        if (attempt == null)
        {
            return Result.Failure<bool>(Error.Failure("Attempt.NotFound", "Quiz attempt not found"));
        }

        if (attempt.status != "in_progress")
        {
            return Result.Failure<bool>(Error.Failure("Attempt.NotInProgress", "Quiz attempt is not in progress"));
        }

        // Get question details for scoring
        const string questionSql = @"
            SELECT question_id, question_type, points
            FROM programs.table_quiz_questions
            WHERE question_id = @question_id";

        var question = await connection.QueryFirstOrDefaultAsync(
            questionSql,
            new { question_id = request.question_id });

        if (question == null)
        {
            return Result.Failure<bool>(Error.Failure("Question.NotFound", "Question not found"));
        }

        // Get correct answers for scoring
        const string correctAnswersSql = @"
            SELECT answer_id, answer_text, is_correct
            FROM programs.table_quiz_answers
            WHERE question_id = @question_id";

        var answers = await connection.QueryAsync(
            correctAnswersSql,
            new { question_id = request.question_id });

        // Calculate score
        bool isCorrect = false;
        decimal pointsEarned = 0;

        switch (question.question_type)
        {
            case "multiple_choice":
            case "true_false":
                var correctAnswerId = answers.Where(a => a.is_correct).Select(a => (Guid)a.answer_id).FirstOrDefault();
                isCorrect = request.selected_answer_ids?.Count == 1 &&
                           request.selected_answer_ids.Contains(correctAnswerId);
                break;

            case "multiple_select":
                var correctAnswerIds = answers.Where(a => a.is_correct).Select(a => (Guid)a.answer_id).ToHashSet();
                var selectedIds = request.selected_answer_ids?.ToHashSet() ?? new HashSet<Guid>();
                isCorrect = correctAnswerIds.SetEquals(selectedIds);
                break;

            case "fill_blank":
                var correctText = answers.Where(a => a.is_correct).Select(a => (string)a.answer_text).FirstOrDefault();
                isCorrect = !string.IsNullOrEmpty(request.text_response) &&
                           request.text_response.Trim().Equals(correctText?.Trim(), StringComparison.OrdinalIgnoreCase);
                break;
        }

        pointsEarned = isCorrect ? question.points : 0;

        // Check if response already exists (update) or create new
        const string checkResponseSql = @"
            SELECT response_id
            FROM programs.table_quiz_responses
            WHERE attempt_id = @attempt_id AND question_id = @question_id";

        var existingResponseId = await connection.QueryFirstOrDefaultAsync<Guid?>(
            checkResponseSql,
            new { attempt_id = request.attempt_id, question_id = request.question_id });

        var now = DateTime.UtcNow;
        var selectedAnswerIdsJson = request.selected_answer_ids != null ?
            JsonSerializer.Serialize(request.selected_answer_ids) : null;

        if (existingResponseId.HasValue)
        {
            // Update existing response
            const string updateResponseSql = @"
                UPDATE programs.table_quiz_responses SET
                    selected_answer_ids = @selected_answer_ids,
                    text_response = @text_response,
                    is_correct = @is_correct,
                    points_earned = @points_earned,
                    points_possible = @points_possible,
                    time_spent_seconds = @time_spent_seconds,
                    answered_at = @answered_at,
                    updated_at = @updated_at
                WHERE response_id = @response_id";

            await connection.ExecuteAsync(updateResponseSql, new
            {
                response_id = existingResponseId.Value,
                selected_answer_ids = selectedAnswerIdsJson,
                text_response = request.text_response,
                is_correct = isCorrect,
                points_earned = pointsEarned,
                points_possible = question.points,
                time_spent_seconds = request.time_spent_seconds,
                answered_at = now,
                updated_at = now
            });
        }
        else
        {
            // Create new response
            var response = new QuizResponse
            {
                response_id = Guid.NewGuid(),
                attempt_id = request.attempt_id,
                question_id = request.question_id,
                selected_answer_ids = selectedAnswerIdsJson,
                text_response = request.text_response,
                is_correct = isCorrect,
                points_earned = pointsEarned,
                points_possible = question.points,
                time_spent_seconds = request.time_spent_seconds,
                answered_at = now,
                created_at = now,
                updated_at = now
            };

            const string insertResponseSql = @"
                INSERT INTO programs.table_quiz_responses (
                    response_id, attempt_id, question_id, selected_answer_ids,
                    text_response, is_correct, points_earned, points_possible,
                    time_spent_seconds, answered_at, created_at, updated_at
                ) VALUES (
                    @response_id, @attempt_id, @question_id, @selected_answer_ids,
                    @text_response, @is_correct, @points_earned, @points_possible,
                    @time_spent_seconds, @answered_at, @created_at, @updated_at
                )";

            await connection.ExecuteAsync(insertResponseSql, response);
        }

        return true;
    }
}
