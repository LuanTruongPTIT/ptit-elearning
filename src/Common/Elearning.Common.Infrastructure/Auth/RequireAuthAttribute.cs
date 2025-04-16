using Microsoft.AspNetCore.Authorization;

namespace Elearning.Common.Infrastructure.Auth;


public class RequireAuthAttribute : AuthorizeAttribute
{
  public RequireAuthAttribute()
  {

  }
}