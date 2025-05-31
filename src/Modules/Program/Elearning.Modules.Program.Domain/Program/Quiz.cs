namespace Elearning.Modules.Program.Domain.Program;

public class Quiz
{
    public Guid quiz_id { get; set; }
    public Guid assignment_id { get; set; }
    public string quiz_title { get; set; }
    public string quiz_description { get; set; }
    
    // Quiz Configuration
    public int? time_limit_minutes { get; set; }
    public int max_attempts { get; set; } = 1;
    public bool shuffle_questions { get; set; } = false;
    public bool shuffle_answers { get; set; } = false;
    public bool show_results_immediately { get; set; } = true;
    public bool show_correct_answers { get; set; } = true;
    public decimal? passing_score_percentage { get; set; }
    public bool allow_review { get; set; } = true;
    public bool auto_submit_on_timeout { get; set; } = true;
    
    // Calculated Fields
    public decimal total_points { get; set; } = 0;
    public int total_questions { get; set; } = 0;
    
    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
