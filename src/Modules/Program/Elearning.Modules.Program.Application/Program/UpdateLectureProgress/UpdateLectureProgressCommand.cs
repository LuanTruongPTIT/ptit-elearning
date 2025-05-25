using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.UpdateLectureProgress;

public sealed record UpdateLectureProgressCommand : ICommand<bool>
{
    public Guid LectureId { get; init; }
    public Guid StudentId { get; init; }
    public int WatchPosition { get; init; } // Position in seconds
    public int ProgressPercentage { get; init; } // Percentage watched (0-100)
}
