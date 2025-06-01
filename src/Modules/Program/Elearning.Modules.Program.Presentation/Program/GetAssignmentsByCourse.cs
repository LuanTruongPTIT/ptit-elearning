using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetAssignmentsByCourse;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class GetAssignmentsByCourse : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/courses/{courseId}/assignments", async (
        Guid courseId,
        ISender sender,
        HttpContext httpContext) =>
    {
      try
      {
        var teacherId = Guid.Parse(httpContext.User.GetUserId());
        var userRoles = httpContext.User.GetUserRoles();

        // Check if user is teacher
        if (!userRoles.Contains("Teacher") && !userRoles.Contains("Lecturer"))
        {
          return Results.Forbid();
        }

        var result = await sender.Send(new GetAssignmentsByCourseQuery(courseId, teacherId));

        return result.Match(
                data => Results.Ok(new
                {
                  status = StatusCodes.Status200OK,
                  message = "Assignments retrieved successfully",
                  data
                }),
                ApiResults.Problem
            );
      }
      catch (Exception ex)
      {
        return Results.Problem(
                title: "An unexpected error occurred",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
      }
    })
    .RequireAuthorization()
    .WithTags(Tags.Program);
  }
}
