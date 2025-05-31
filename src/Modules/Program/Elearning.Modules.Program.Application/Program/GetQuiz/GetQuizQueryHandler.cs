using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetQuiz;

internal sealed class GetQuizQueryHandler : IQueryHandler<GetQuizQuery, GetQuizResponse>
{
    private readonly IDbConnectionFactory dbConnectionFactory;

    public GetQuizQueryHandler(IDbConnectionFactory _dbConnectionFactory)
    {
        dbConnectionFactory = _dbConnectionFactory;
    }

    public async Task<Result<GetQuizResponse>> Handle(GetQuizQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
        // Get quiz
        const string quizSql = @"
            SELECT quiz_id, assignment_id, quiz_title, quiz_description,
                   time_limit_minutes, max_attempts, shuffle_questions, shuffle_answers,
                   show_results_immediately, show_correct_answers, passing_score_percentage,
                   allow_review, auto_submit_on_timeout, total_points, total_questions,
                   created_at, created_by
            FROM programs.table_quizzes
            WHERE quiz_id = @quiz_id";

        var quiz = await connection.QueryFirstOrDefaultAsync(quizSql, new { quiz_id = request.quiz_id });

        if (quiz == null)
        {
            return Result.Failure<GetQuizResponse>(Error.Failure("Quiz.NotFound", "Quiz not found"));
        }

        // Get questions with answers
        const string questionsSql = @"
            SELECT q.question_id, q.question_text, q.question_type, q.points,
                   q.question_order, q.explanation, q.is_required, q.randomize_answers,
                   a.answer_id, a.answer_text, a.is_correct, a.answer_order, a.answer_explanation
            FROM programs.table_quiz_questions q
            LEFT JOIN programs.table_quiz_answers a ON q.question_id = a.question_id
            WHERE q.quiz_id = @quiz_id
            ORDER BY q.question_order, a.answer_order";

        var questionAnswerData = await connection.QueryAsync(questionsSql, new { quiz_id = request.quiz_id });

        // Group questions and answers
        var questions = questionAnswerData
            .GroupBy(row => new
            {
                question_id = (Guid)row.question_id,
                question_text = (string)row.question_text,
                question_type = (string)row.question_type,
                points = (decimal)row.points,
                question_order = (int)row.question_order,
                explanation = (string)row.explanation,
                is_required = (bool)row.is_required,
                randomize_answers = (bool)row.randomize_answers
            })
            .Select(g => new QuizQuestionResponse(
                g.Key.question_id,
                g.Key.question_text,
                g.Key.question_type,
                g.Key.points,
                g.Key.question_order,
                g.Key.explanation,
                g.Key.is_required,
                g.Key.randomize_answers,
                g.Where(row => row.answer_id != null)
                 .Select(row => new QuizAnswerResponse(
                     (Guid)row.answer_id,
                     (string)row.answer_text,
                     (bool)row.is_correct,
                     (int)row.answer_order,
                     (string)row.answer_explanation
                 ))
                 .ToList()
            ))
            .ToList();

        var response = new GetQuizResponse(
            (Guid)quiz.quiz_id,
            (Guid)quiz.assignment_id,
            (string)quiz.quiz_title,
            (string)quiz.quiz_description,
            (int?)quiz.time_limit_minutes,
            (int)quiz.max_attempts,
            (bool)quiz.shuffle_questions,
            (bool)quiz.shuffle_answers,
            (bool)quiz.show_results_immediately,
            (bool)quiz.show_correct_answers,
            (decimal?)quiz.passing_score_percentage,
            (bool)quiz.allow_review,
            (bool)quiz.auto_submit_on_timeout,
            (decimal)quiz.total_points,
            (int)quiz.total_questions,
            (DateTime)quiz.created_at,
            (Guid)quiz.created_by,
            questions
        );

        return response;
    }
}
