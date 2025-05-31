namespace Elearning.Modules.Program.Domain.Program;

public class QuizBankQuestion
{
    public Guid bank_question_id { get; set; }
    public Guid bank_id { get; set; }
    public string question_text { get; set; }
    public string question_type { get; set; } // 'multiple_choice', 'multiple_select', 'true_false', 'fill_blank'
    public decimal default_points { get; set; } = 1.0m;
    public string explanation { get; set; }
    
    // Question metadata
    public string difficulty_level { get; set; } // 'easy', 'medium', 'hard'
    public string tags { get; set; } // JSON array of tags for categorization
    
    // Usage statistics
    public int usage_count { get; set; } = 0;
    
    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
