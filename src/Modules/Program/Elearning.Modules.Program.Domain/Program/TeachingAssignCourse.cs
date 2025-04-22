namespace Elearning.Modules.Program.Domain.Program;

public class TeachingAssignCourse
{
  public Guid id { get; set; }
  public Guid teacher_id { get; set; }
  public required string course_class_name { get; set; }
  public string description { get; set; }
  public Guid class_id { get; set; }
  public Guid course_id { get; set; }
  public DateTime start_date { get; set; }
  public DateTime end_date { get; set; }
  public string status { get; set; } = "active";
  public DateTime created_at { get; set; }
  public DateTime updated_at { get; set; }
}