namespace Elearning.Modules.Program.Domain.Program;

public class QuizResponse
{
    public Guid response_id { get; set; }
    public Guid attempt_id { get; set; }
    public Guid question_id { get; set; }

    // Response Data (stored as JSON)
    public string selected_answer_ids { get; set; } // JSON array of selected answer IDs
    public string text_response { get; set; } // For fill-in-the-blank questions

    // Scoring
    public bool is_correct { get; set; } = false;
    public decimal points_earned { get; set; } = 0;
    public decimal points_possible { get; set; } = 0;

    // Timing
    public int? time_spent_seconds { get; set; }
    public DateTime? answered_at { get; set; }

    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
}
