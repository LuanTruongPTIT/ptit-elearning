namespace Elearning.Modules.Program.Application.Program.GetClassByDepartmentOfTeacher;


public sealed class GetClassByDepartmentOfTeacherResponse
{
  public Guid class_id { get; set; }
  public string class_name { get; set; }
  public Guid department_id { get; set; }
  public Guid program_id { get; set; }
  public string program_name { get; set; }
  public Guid teacher_id { get; set; }
}
