using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.SubmitQuizResponse;

public sealed record SubmitQuizResponseCommand(
    Guid attempt_id,
    Guid question_id,
    List<Guid> selected_answer_ids,
    string text_response,
    int? time_spent_seconds
) : ICommand<bool>;
