namespace Elearning.Modules.Program.Domain.Program;

public class Lecture
{
  public Guid id { get; set; }
  public Guid course_id { get; set; }
  public Guid teaching_assign_course_id { get; set; }
  public string title { get; set; }
  public string description { get; set; }
  public string content_type { get; set; } // 'VIDEO_UPLOAD' or 'YOUTUBE_LINK'
  public string content_url { get; set; } // URL đến file video hoặc YouTube link
  public string youtube_video_id { get; set; } // ID của video YouTube (nếu là YouTube link)
  public int? duration { get; set; } // Thời lượng video (tính bằng giây)
  public bool is_published { get; set; }
  public DateTime created_at { get; set; }
  public DateTime updated_at { get; set; }
  public Guid created_by { get; set; }
}
