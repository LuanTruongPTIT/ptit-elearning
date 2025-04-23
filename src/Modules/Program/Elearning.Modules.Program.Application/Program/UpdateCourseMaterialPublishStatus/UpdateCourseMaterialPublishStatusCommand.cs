using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.UpdateCourseMaterialPublishStatus;

public sealed record UpdateCourseMaterialPublishStatusCommand(
    Guid id,
    bool is_published
) : ICommand<bool>;
