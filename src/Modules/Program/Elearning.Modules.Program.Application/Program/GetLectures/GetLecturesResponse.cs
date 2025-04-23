namespace Elearning.Modules.Program.Application.Program.GetLectures;

public sealed record GetLecturesResponse(
    Guid id,
    Guid course_id,
    Guid teaching_assign_course_id,
    string title,
    string description,
    string content_type,
    string content_url,
    string youtube_video_id,
    int? duration,
    bool is_published,
    DateTime created_at,
    DateTime updated_at,
    Guid created_by
);
