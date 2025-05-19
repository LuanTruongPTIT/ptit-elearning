using System;

namespace Elearning.Modules.Program.Domain.Program;

public class StudentCourseProgress
{
    public Guid id { get; set; }
    public Guid student_id { get; set; }
    public Guid teaching_assign_course_id { get; set; }
    public int total_lectures { get; set; }
    public int completed_lectures { get; set; }
    public int progress_percentage { get; set; }
    public DateTime last_accessed { get; set; }
    public string status { get; set; } = "in_progress"; // in_progress, completed, not_started
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}
