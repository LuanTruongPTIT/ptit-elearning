using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetQuizAttempt;

internal sealed class GetQuizAttemptQueryHandler : IQueryHandler<GetQuizAttemptQuery, GetQuizAttemptResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public GetQuizAttemptQueryHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<GetQuizAttemptResponse>> Handle(GetQuizAttemptQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    // Get attempt details
    const string attemptSql = @"
            SELECT qa.attempt_id, qa.quiz_id, qa.student_id, qa.attempt_number, qa.status,
                   qa.started_at, qa.submitted_at, qa.time_taken_seconds,
                   qa.total_score, qa.max_possible_score, qa.percentage_score, qa.passed,
                   q.time_limit_minutes
            FROM programs.table_quiz_attempts qa
            JOIN programs.table_quizzes q ON qa.quiz_id = q.quiz_id
            WHERE qa.attempt_id = @attempt_id";

    var attempt = await connection.QueryFirstOrDefaultAsync(attemptSql, new { attempt_id = request.attempt_id });

    if (attempt == null)
    {
      return Result.Failure<GetQuizAttemptResponse>(Error.Failure("Attempt.NotFound", "Quiz attempt not found"));
    }

    // Calculate remaining time
    int? remainingTimeSeconds = null;
    if (attempt.status == "in_progress" && attempt.time_limit_minutes != null)
    {
      var elapsed = DateTime.UtcNow - (DateTime)attempt.started_at;
      var timeLimit = TimeSpan.FromMinutes((int)attempt.time_limit_minutes);
      var remaining = timeLimit - elapsed;
      remainingTimeSeconds = remaining.TotalSeconds > 0 ? (int)remaining.TotalSeconds : 0;
    }

    // Get responses
    const string responsesSql = @"
            SELECT qr.response_id, qr.question_id, qr.selected_answer_ids, qr.text_response,
                   qr.is_correct, qr.points_earned, qr.points_possible, qr.time_spent_seconds, qr.answered_at,
                   qq.question_text, qq.question_type
            FROM programs.table_quiz_responses qr
            JOIN programs.table_quiz_questions qq ON qr.question_id = qq.question_id
            WHERE qr.attempt_id = @attempt_id
            ORDER BY qq.question_order";

    var responses = await connection.QueryAsync(responsesSql, new { attempt_id = request.attempt_id });

    var responseDetails = responses.Select(r => new QuizAttemptResponseDetail(
        (Guid)r.response_id,
        (Guid)r.question_id,
        (string)r.question_text,
        (string)r.question_type,
        (decimal)r.points_possible,
        (string)r.selected_answer_ids,
        (string)r.text_response,
        (bool)r.is_correct,
        (decimal)r.points_earned,
        (int?)r.time_spent_seconds,
        (DateTime?)r.answered_at
    )).ToList();

    var response = new GetQuizAttemptResponse(
        (Guid)attempt.attempt_id,
        (Guid)attempt.quiz_id,
        (Guid)attempt.student_id,
        (int)attempt.attempt_number,
        (string)attempt.status,
        (DateTime)attempt.started_at,
        (DateTime?)attempt.submitted_at,
        (int?)attempt.time_taken_seconds,
        (decimal)attempt.total_score,
        (decimal)attempt.max_possible_score,
        (decimal)attempt.percentage_score,
        (bool?)attempt.passed,
        remainingTimeSeconds,
        responseDetails
    );

    return response;
  }
}
