using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.SubmitQuizAttempt;

internal sealed class SubmitQuizAttemptCommandHandler : ICommandHandler<SubmitQuizAttemptCommand, SubmitQuizAttemptResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public SubmitQuizAttemptCommandHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<SubmitQuizAttemptResponse>> Handle(SubmitQuizAttemptCommand request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    // Get attempt details
    const string getAttemptSql = @"
            SELECT qa.attempt_id, qa.quiz_id, qa.student_id, qa.status, qa.started_at,
                   q.passing_score_percentage
            FROM programs.table_quiz_attempts qa
            JOIN programs.table_quizzes q ON qa.quiz_id = q.quiz_id
            WHERE qa.attempt_id = @attempt_id";

    var attempt = await connection.QueryFirstOrDefaultAsync(getAttemptSql, new { attempt_id = request.attempt_id });

    if (attempt == null)
    {
      return Result.Failure<SubmitQuizAttemptResponse>(Error.Failure("Attempt.NotFound", "Quiz attempt not found"));
    }

    if (attempt.status != "in_progress" && !request.force_submit)
    {
      return Result.Failure<SubmitQuizAttemptResponse>(Error.Failure("Attempt.NotInProgress", "Quiz attempt is not in progress"));
    }

    var now = DateTime.UtcNow;
    var timeTakenSeconds = (int)(now - (DateTime)attempt.started_at).TotalSeconds;

    // Calculate total score from responses
    const string calculateScoreSql = @"
            SELECT
                COALESCE(SUM(points_earned), 0) as total_score,
                COALESCE(SUM(points_possible), 0) as max_possible_score
            FROM programs.table_quiz_responses
            WHERE attempt_id = @attempt_id";

    var scoreResult = await connection.QueryFirstOrDefaultAsync(calculateScoreSql, new { attempt_id = request.attempt_id });

    var totalScore = (decimal)(scoreResult?.total_score ?? 0);
    var maxPossibleScore = (decimal)(scoreResult?.max_possible_score ?? 0);
    var percentageScore = maxPossibleScore > 0 ? (totalScore / maxPossibleScore) * 100 : 0;

    // Determine if passed
    bool? passed = null;
    if (attempt.passing_score_percentage != null)
    {
      passed = percentageScore >= (decimal)attempt.passing_score_percentage;
    }

    // Update attempt
    const string updateAttemptSql = @"
            UPDATE programs.table_quiz_attempts SET
                status = @status,
                submitted_at = @submitted_at,
                time_taken_seconds = @time_taken_seconds,
                total_score = @total_score,
                max_possible_score = @max_possible_score,
                percentage_score = @percentage_score,
                passed = @passed,
                updated_at = @updated_at
            WHERE attempt_id = @attempt_id";

    await connection.ExecuteAsync(updateAttemptSql, new
    {
      attempt_id = request.attempt_id,
      status = request.force_submit ? "auto_submitted" : "submitted",
      submitted_at = now,
      time_taken_seconds = timeTakenSeconds,
      total_score = totalScore,
      max_possible_score = maxPossibleScore,
      percentage_score = percentageScore,
      passed = passed,
      updated_at = now
    });

    return new SubmitQuizAttemptResponse(
        (Guid)attempt.attempt_id,
        totalScore,
        maxPossibleScore,
        percentageScore,
        passed,
        now,
        timeTakenSeconds
    );
  }
}
