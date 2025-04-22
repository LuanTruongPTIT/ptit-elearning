using Microsoft.AspNetCore.Authorization;

namespace Elearning.Common.Infrastructure.Auth;


public class RequireAuthAttribute : AuthorizeAttribute
{
  public RequireAuthAttribute()
  {
    AuthenticationSchemes = "Bearer"; // Specify the 
  }

  public RequireAuthAttribute(string role) : base()
  {
    AuthenticationSchemes = "Bearer"; // Specify the authentication scheme
    Policy = $"RequireRole_{role}";
    Console.WriteLine("role", role);
    if (!IsValidRole(role))
    {
      throw new ArgumentException($"Invalid role name: {role}. Valid roles are: Administrator, Teacher, Student");
    }
  }

  private bool IsValidRole(string role)
  {
    return role is "Admin" or "Teacher" or "Student";
  }
}