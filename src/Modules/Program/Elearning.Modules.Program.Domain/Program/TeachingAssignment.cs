namespace Elearning.Modules.Program.Domain.Program;

public class TeachingAssignment
{
  public Guid id { get; set; }
  public Guid teacher_id { get; set; }
  public Guid department_id { get; set; }
  public List<Guid> subjects { get; set; }
  public DateTime employed_date { get; set; }
  public DateTime created_at { get; set; }
  public DateTime updated_at { get; set; }

}