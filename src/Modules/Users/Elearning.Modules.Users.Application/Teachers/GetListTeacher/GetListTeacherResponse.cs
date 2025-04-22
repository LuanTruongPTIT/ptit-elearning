namespace Elearning.Modules.Users.Application.Teachers.GetListTeacher;


public class GetListTeacherResponse
{
  public Guid user_id { get; set; }
  public string full_name { get; set; }
  public string email { get; set; }
  public string? phoneNumber { get; set; }
  public string? address { get; set; }
  public string gender { get; set; }
  public string role_name { get; set; }
  public TeacherInformationTeaching information_teaching { get; set; }
}

public class TeacherInformationTeaching
{
  public Guid department_id { get; set; }
  public string department_name { get; set; }
  public string department_code { get; set; }
  public List<TeacherInformationSubjectAssigned> courses { get; set; }

}
public class TeacherInformationSubjectAssigned
{
  public Guid course_id { get; set; }
  public string course_name { get; set; }
  public string course_code { get; set; }
}