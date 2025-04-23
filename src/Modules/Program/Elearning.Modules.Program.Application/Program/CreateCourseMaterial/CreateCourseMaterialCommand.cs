using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.CreateCourseMaterial;

public sealed record CreateCourseMaterialCommand(
    Guid course_id,
    string title,
    string description,
    string file_url,
    string file_type,
    long file_size,
    bool is_published,
    string youtube_video_id,
    string content_type
) : ICommand<Guid>
{
    public Guid created_by { get; set; }
}
