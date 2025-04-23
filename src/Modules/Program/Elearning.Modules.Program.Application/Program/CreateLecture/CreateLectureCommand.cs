using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.CreateLecture;

public sealed record CreateLectureCommand(
    Guid teaching_assign_course_id,
    string title,
    string description,
    string content_type,
    string content_url,
    string youtube_video_id,
    int? duration,
    bool is_published
) : ICommand<Guid>
{
  public Guid created_by { get; set; }
}
