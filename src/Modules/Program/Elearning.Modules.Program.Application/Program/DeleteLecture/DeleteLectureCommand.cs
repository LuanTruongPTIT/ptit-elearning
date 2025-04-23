using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.DeleteLecture;

public sealed record DeleteLectureCommand(
    Guid id
) : ICommand<bool>;
