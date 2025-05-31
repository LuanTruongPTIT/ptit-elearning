namespace Elearning.Modules.Program.Domain.Program;

public class QuizQuestionBank
{
    public Guid bank_id { get; set; }
    public string bank_name { get; set; }
    public string bank_description { get; set; }
    public string subject_area { get; set; }
    public string difficulty_level { get; set; } // 'beginner', 'intermediate', 'advanced', 'expert'
    
    // Sharing Settings
    public bool is_public { get; set; } = false;
    public bool is_system_bank { get; set; } = false;
    
    // Statistics
    public int question_count { get; set; } = 0;
    public int usage_count { get; set; } = 0;
    
    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
