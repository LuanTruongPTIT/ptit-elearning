namespace Elearning.Modules.Program.Domain.Program;

public class QuizBankAnswer
{
    public Guid bank_answer_id { get; set; }
    public Guid bank_question_id { get; set; }
    public string answer_text { get; set; }
    public bool is_correct { get; set; } = false;
    public int answer_order { get; set; }
    public string answer_explanation { get; set; }
    
    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
