using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Users.Application.Users.CreateTeacher;


public sealed class CreateTeacherCommand() : ICommand<string>
{
  public string address { get; set; }
  public DateTime birthday { get; set; }
  public Guid department { get; set; }
  public string email { get; set; }
  public string fullName { get; set; }
  public string gender { get; set; }
  public string password { get; set; }
  public string phone { get; set; }
  public List<Guid> subjects { get; set; }
  public string username { get; set; }
  public string status { get; set; }
  public DateTime employmentDate { get; set; }
}
