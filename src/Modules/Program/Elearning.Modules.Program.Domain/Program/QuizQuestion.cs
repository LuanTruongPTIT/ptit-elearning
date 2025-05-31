namespace Elearning.Modules.Program.Domain.Program;

public class QuizQuestion
{
    public Guid question_id { get; set; }
    public Guid quiz_id { get; set; }
    public string question_text { get; set; }
    public string question_type { get; set; } // 'multiple_choice', 'multiple_select', 'true_false', 'fill_blank'
    public decimal points { get; set; } = 1.0m;
    public int question_order { get; set; }
    public string explanation { get; set; }
    public bool is_required { get; set; } = true;
    public bool randomize_answers { get; set; } = false;

    // Audit Fields
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Guid created_by { get; set; }
}
