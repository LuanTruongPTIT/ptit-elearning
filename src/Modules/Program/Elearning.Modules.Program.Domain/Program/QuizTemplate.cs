namespace Elearning.Modules.Program.Domain.Program;

public class QuizTemplate
{
    public Guid template_id { get; set; }
    public string template_name { get; set; }
    public string template_description { get; set; }
    public string category { get; set; }
    
    // Template Data (JSON structure)
    public string template_data { get; set; }
    
    // Sharing Settings
    public bool is_public { get; set; } = false;
    public bool is_system_template { get; set; } = false;
    
    // Usage Statistics
    public int usage_count { get; set; } = 0;
    
    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
