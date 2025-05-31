using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Modules.Program.Domain.Program;

namespace Elearning.Modules.Program.Application.Program.CreateQuiz;

internal sealed class CreateQuizCommandHandler : ICommandHandler<CreateQuizCommand, Guid>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public CreateQuizCommandHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<Guid>> Handle(CreateQuizCommand request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

        var quizId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // Create quiz
        var quiz = new Quiz
        {
            quiz_id = quizId,
            assignment_id = request.assignment_id,
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
            total_points = request.questions.Sum(q => q.points),
            total_questions = request.questions.Count,
            created_at = now,
            updated_at = now,
            created_by = request.created_by
        };

        const string insertQuizSql = @"
            INSERT INTO programs.table_quizzes (
                quiz_id, assignment_id, quiz_title, quiz_description,
                time_limit_minutes, max_attempts, shuffle_questions, shuffle_answers,
                show_results_immediately, show_correct_answers, passing_score_percentage,
                allow_review, auto_submit_on_timeout, total_points, total_questions,
                created_at, updated_at, created_by
            ) VALUES (
                @quiz_id, @assignment_id, @quiz_title, @quiz_description,
                @time_limit_minutes, @max_attempts, @shuffle_questions, @shuffle_answers,
                @show_results_immediately, @show_correct_answers, @passing_score_percentage,
                @allow_review, @auto_submit_on_timeout, @total_points, @total_questions,
                @created_at, @updated_at, @created_by
            )";

        await connection.ExecuteAsync(insertQuizSql, quiz);

        // Create questions and answers
        foreach (var questionRequest in request.questions)
        {
            var questionId = Guid.NewGuid();

            var question = new QuizQuestion
            {
                question_id = questionId,
                quiz_id = quizId,
                question_text = questionRequest.question_text,
                question_type = questionRequest.question_type,
                points = questionRequest.points,
                question_order = questionRequest.question_order,
                explanation = questionRequest.explanation,
                is_required = questionRequest.is_required,
                randomize_answers = questionRequest.randomize_answers,
                created_at = now,
                updated_at = now,
                created_by = request.created_by
            };

            const string insertQuestionSql = @"
                INSERT INTO programs.table_quiz_questions (
                    question_id, quiz_id, question_text, question_type, points,
                    question_order, explanation, is_required, randomize_answers,
                    created_at, updated_at, created_by
                ) VALUES (
                    @question_id, @quiz_id, @question_text, @question_type, @points,
                    @question_order, @explanation, @is_required, @randomize_answers,
                    @created_at, @updated_at, @created_by
                )";

            await connection.ExecuteAsync(insertQuestionSql, question);

            // Create answers
            foreach (var answerRequest in questionRequest.answers)
            {
                var answer = new QuizAnswer
                {
                    answer_id = Guid.NewGuid(),
                    question_id = questionId,
                    answer_text = answerRequest.answer_text,
                    is_correct = answerRequest.is_correct,
                    answer_order = answerRequest.answer_order,
                    answer_explanation = answerRequest.answer_explanation,
                    created_at = now,
                    updated_at = now,
                    created_by = request.created_by
                };

                const string insertAnswerSql = @"
                    INSERT INTO programs.table_quiz_answers (
                        answer_id, question_id, answer_text, is_correct,
                        answer_order, answer_explanation, created_at, updated_at, created_by
                    ) VALUES (
                        @answer_id, @question_id, @answer_text, @is_correct,
                        @answer_order, @answer_explanation, @created_at, @updated_at, @created_by
                    )";

                await connection.ExecuteAsync(insertAnswerSql, answer);
            }
        }

        return quizId;
    }
}
