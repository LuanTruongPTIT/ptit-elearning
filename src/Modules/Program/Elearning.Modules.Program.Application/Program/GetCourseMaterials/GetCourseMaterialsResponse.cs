namespace Elearning.Modules.Program.Application.Program.GetCourseMaterials;

public sealed record GetCourseMaterialsResponse(
    Guid id,
    Guid course_id,
    string title,
    string description,
    string file_url,
    string file_type,
    long file_size,
    bool is_published,
    DateTime created_at,
    DateTime updated_at,
    Guid created_by,
    string youtube_video_id,
    string content_type
);
