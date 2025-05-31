using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.UpdateQuiz;

public sealed record UpdateQuizCommand(
    Guid quiz_id,
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
    bool auto_submit_on_timeout
) : ICommand<bool>;
