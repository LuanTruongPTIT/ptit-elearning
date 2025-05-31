using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetRecentActivities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher,Student")]
internal sealed class GetRecentActivities : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/recent-activities", async (
        Guid? userId,
        int limit,
        int offset,
        ISender sender,
        HttpContext httpContext) =>
    {
      // If userId is not provided, use current user's ID
      var currentUserId = Guid.Parse(httpContext.User.GetUserId());
      var userRoles = httpContext.User.GetUserRoles();
      var isTeacher = userRoles.Contains("Teacher") || userRoles.Contains("Lecturer");
      var isAdmin = userRoles.Contains("Administrator");

      // Students can only see their own activities
      // Teachers and Admins can see any user's activities
      Guid? targetUserId = null;
      if (userId.HasValue)
      {
        if (isTeacher || isAdmin || userId.Value == currentUserId)
        {
          targetUserId = userId.Value;
        }
        else
        {
          return Results.Forbid();
        }
      }
      else
      {
        targetUserId = currentUserId;
      }

      var query = new GetRecentActivitiesQuery(targetUserId, limit, offset);
      var result = await sender.Send(query);

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Get recent activities successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization()
    .WithTags("Program");
  }
}
