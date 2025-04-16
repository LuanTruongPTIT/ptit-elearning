namespace Elearning.Modules.Program.Application.Program;

public sealed class ProgramGetCourseDepartmentResponse
{
  public Guid course_id { get; set; }
  public string course_name { get; set; }
  public Guid department_id { get; set; }
  public string department_name { get; set; }
}
