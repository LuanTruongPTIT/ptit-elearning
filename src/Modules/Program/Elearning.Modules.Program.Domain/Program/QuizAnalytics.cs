namespace Elearning.Modules.Program.Domain.Program;

public class QuizAnalytics
{
    public Guid analytics_id { get; set; }
    public Guid quiz_id { get; set; }
    
    // Time Period
    public string period_type { get; set; } // 'daily', 'weekly', 'monthly'
    public DateTime period_date { get; set; }
    
    // Attempt Statistics
    public int total_attempts { get; set; } = 0;
    public int completed_attempts { get; set; } = 0;
    public int in_progress_attempts { get; set; } = 0;
    
    // Score Statistics
    public decimal average_score { get; set; } = 0;
    public decimal highest_score { get; set; } = 0;
    public decimal lowest_score { get; set; } = 0;
    public decimal median_score { get; set; } = 0;
    
    // Time Statistics
    public int average_time_seconds { get; set; } = 0;
    public int fastest_time_seconds { get; set; } = 0;
    public int slowest_time_seconds { get; set; } = 0;
    
    // Pass/Fail Statistics
    public int passed_count { get; set; } = 0;
    public int failed_count { get; set; } = 0;
    public decimal pass_rate_percentage { get; set; } = 0;
    
    // Question-level Statistics (JSON)
    public string question_statistics { get; set; }
    
    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
