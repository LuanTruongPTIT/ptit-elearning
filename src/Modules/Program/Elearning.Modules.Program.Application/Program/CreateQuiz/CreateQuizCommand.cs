using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.CreateQuiz;

public sealed record CreateQuizCommand(
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
    List<CreateQuizQuestionRequest> questions
) : ICommand<Guid>
{
    public Guid created_by { get; set; }
}

public sealed record CreateQuizQuestionRequest(
    string question_text,
    string question_type,
    decimal points,
    int question_order,
    string explanation,
    bool is_required,
    bool randomize_answers,
    List<CreateQuizAnswerRequest> answers
);

public sealed record CreateQuizAnswerRequest(
    string answer_text,
    bool is_correct,
    int answer_order,
    string answer_explanation
);
