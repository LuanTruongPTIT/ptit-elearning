using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.SubmitQuizAttempt;

public sealed record SubmitQuizAttemptCommand(
    Guid attempt_id,
    bool force_submit
) : ICommand<SubmitQuizAttemptResponse>;

public sealed record SubmitQuizAttemptResponse(
    Guid attempt_id,
    decimal total_score,
    decimal max_possible_score,
    decimal percentage_score,
    bool? passed,
    DateTime submitted_at,
    int time_taken_seconds
);
