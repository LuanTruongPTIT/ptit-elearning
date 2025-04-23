namespace Elearning.Modules.Program.Domain.Program;

public class CourseMaterial
{
    public Guid id { get; set; }
    public Guid course_id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string file_url { get; set; }
    public string file_type { get; set; }
    public long file_size { get; set; }
    public bool is_published { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
    public string youtube_video_id { get; set; }
    public string content_type { get; set; } // "VIDEO_UPLOAD" or "YOUTUBE_LINK"
}
