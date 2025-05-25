using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.MarkLectureCompleted;

public sealed record MarkLectureCompletedCommand : ICommand<bool>
{
    public Guid LectureId { get; init; }
    public Guid StudentId { get; init; }
}
