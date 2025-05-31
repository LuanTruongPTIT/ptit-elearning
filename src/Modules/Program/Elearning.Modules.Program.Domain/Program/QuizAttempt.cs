namespace Elearning.Modules.Program.Domain.Program;

public class QuizAttempt
{
    public Guid attempt_id { get; set; }
    public Guid quiz_id { get; set; }
    public Guid student_id { get; set; }
    public int attempt_number { get; set; } = 1;
    public string status { get; set; } = "in_progress"; // 'in_progress', 'submitted', 'auto_submitted', 'expired', 'graded'

    // Timing Information
    public DateTime started_at { get; set; }
    public DateTime? submitted_at { get; set; }
    public int? time_taken_seconds { get; set; }

    // Scoring Information
    public decimal total_score { get; set; } = 0;
    public decimal max_possible_score { get; set; } = 0;
    public decimal percentage_score { get; set; } = 0;
    public bool? passed { get; set; }

    // Additional Information
    public string ip_address { get; set; }
    public string user_agent { get; set; }

    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
