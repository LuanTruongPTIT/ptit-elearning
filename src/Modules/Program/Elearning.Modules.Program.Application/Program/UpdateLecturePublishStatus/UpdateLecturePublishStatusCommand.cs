using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.UpdateLecturePublishStatus;

public sealed record UpdateLecturePublishStatusCommand(
    Guid id,
    bool is_published
) : ICommand<bool>;
