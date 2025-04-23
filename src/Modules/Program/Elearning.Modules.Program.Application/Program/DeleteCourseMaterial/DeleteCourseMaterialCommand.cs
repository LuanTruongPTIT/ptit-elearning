using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.DeleteCourseMaterial;

public sealed record DeleteCourseMaterialCommand(
    Guid id
) : ICommand<bool>;
