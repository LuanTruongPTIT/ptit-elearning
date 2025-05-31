using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.UpdateQuiz;

internal sealed class UpdateQuizCommandHandler : ICommandHandler<UpdateQuizCommand, bool>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;

  public UpdateQuizCommandHandler(IDbConnectionFactory dbConnectionFactory)
  {
    _dbConnectionFactory = dbConnectionFactory;
  }

  public async Task<Result<bool>> Handle(UpdateQuizCommand request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    // Check if quiz exists
    const string checkQuizSql = @"
            SELECT quiz_id
            FROM programs.table_quizzes
            WHERE quiz_id = @quiz_id";

    var quizExists = await connection.QueryFirstOrDefaultAsync<Guid?>(checkQuizSql, new { quiz_id = request.quiz_id });

    if (!quizExists.HasValue)
    {
      return Result.Failure<bool>(Error.Failure("Quiz.NotFound", "Quiz not found"));
    }

    // Update quiz
    const string updateQuizSql = @"
            UPDATE programs.table_quizzes SET
                quiz_title = @quiz_title,
                quiz_description = @quiz_description,
                time_limit_minutes = @time_limit_minutes,
                max_attempts = @max_attempts,
                shuffle_questions = @shuffle_questions,
                shuffle_answers = @shuffle_answers,
                show_results_immediately = @show_results_immediately,
                show_correct_answers = @show_correct_answers,
                passing_score_percentage = @passing_score_percentage,
                allow_review = @allow_review,
                auto_submit_on_timeout = @auto_submit_on_timeout,
                updated_at = @updated_at
            WHERE quiz_id = @quiz_id";

    var rowsAffected = await connection.ExecuteAsync(updateQuizSql, new
    {
      quiz_id = request.quiz_id,
      quiz_title = request.quiz_title,
      quiz_description = request.quiz_description,
      time_limit_minutes = request.time_limit_minutes,
      max_attempts = request.max_attempts,
      shuffle_questions = request.shuffle_questions,
      shuffle_answers = request.shuffle_answers,
      show_results_immediately = request.show_results_immediately,
      show_correct_answers = request.show_correct_answers,
      passing_score_percentage = request.passing_score_percentage,
      allow_review = request.allow_review,
      auto_submit_on_timeout = request.auto_submit_on_timeout,
      updated_at = DateTime.UtcNow
    });

    return rowsAffected > 0;
  }
}
