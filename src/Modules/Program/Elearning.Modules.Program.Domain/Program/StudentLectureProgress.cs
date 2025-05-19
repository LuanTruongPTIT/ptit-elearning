using System;

namespace Elearning.Modules.Program.Domain.Program;

public class StudentLectureProgress
{
    public Guid id { get; set; }
    public Guid student_id { get; set; }
    public Guid lecture_id { get; set; }
    public int watch_position { get; set; } // Vị trí đã xem đến (giây)
    public int progress_percentage { get; set; } // Phần trăm hoàn thành
    public bool is_completed { get; set; }
    public DateTime? last_accessed { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}
