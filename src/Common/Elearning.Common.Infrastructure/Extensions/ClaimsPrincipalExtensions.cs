using System.Security.Claims;

namespace Elearning.Common.Infrastructure.Extensions;

public static class ClaimsPrincipalExtensions
{
  public static string GetUserId(this ClaimsPrincipal principal)
  {
    Console.WriteLine("tests", principal.FindFirstValue("sub"));
    return principal.FindFirstValue("sub") ??
           principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
           string.Empty;
  }


  public static IEnumerable<string> GetUserRoles(this ClaimsPrincipal principal)
  {
    return principal.Claims
        .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
        .Select(c => c.Value);
  }
}