using System;

namespace Elearning.Modules.Program.Application.Program.GetStudentCourses;

public sealed class GetStudentCoursesResponse
{
  public Guid course_id { get; set; }
  public string course_name { get; set; } = null!;
  public string course_code { get; set; } = null!;
  public string thumbnail_url { get; set; } = null!;
  public string description { get; set; } = null!;
  public string teacher_name { get; set; } = null!;
  public int progress_percentage { get; set; }
  public int total_lectures { get; set; }
  public int completed_lectures { get; set; }
  public DateTime enrollment_date { get; set; }
  public DateTime? last_accessed { get; set; }
  public string status { get; set; } = null!; // Course status: in_progress, completed, not_started
}
