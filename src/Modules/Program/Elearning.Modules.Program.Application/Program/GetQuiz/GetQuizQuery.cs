using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetQuiz;

public sealed record GetQuizQuery(Guid quiz_id) : IQuery<GetQuizResponse>;

public sealed record GetQuizResponse(
    Guid quiz_id,
    Guid assignment_id,
    string quiz_title,
    string quiz_description,
    int? time_limit_minutes,
    int max_attempts,
    bool shuffle_questions,
    bool shuffle_answers,
    bool show_results_immediately,
    bool show_correct_answers,
    decimal? passing_score_percentage,
    bool allow_review,
    bool auto_submit_on_timeout,
    decimal total_points,
    int total_questions,
    DateTime created_at,
    Guid created_by,
    List<QuizQuestionResponse> questions
);

public sealed record QuizQuestionResponse(
    Guid question_id,
    string question_text,
    string question_type,
    decimal points,
    int question_order,
    string explanation,
    bool is_required,
    bool randomize_answers,
    List<QuizAnswerResponse> answers
);

public sealed record QuizAnswerResponse(
    Guid answer_id,
    string answer_text,
    bool is_correct,
    int answer_order,
    string answer_explanation
);
