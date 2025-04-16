
namespace Elearning.Modules.Users.Domain.Users;

public sealed class Role
{
  public static readonly Role Administrator = new("Administrator");
  public static readonly Role Student = new("Student");
  public static readonly Role Lecturer = new("Lecturer");

  private Role(string name)
  {
    this.name = name;
  }
  private Role() { }

  public string name { get; private set; }
}