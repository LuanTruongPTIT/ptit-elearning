using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetTeacherClasses;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class GetTeacherClasses : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/teacher/classes", async (
        ISender sender,
        HttpContext context) =>
    {
      try
      {
        // Get teacher ID from JWT token
        var teacherId = context.User.GetUserId();
        var userRoles = context.User.GetUserRoles();

        // Check if user is teacher
        if (!userRoles.Contains("Teacher") && !userRoles.Contains("Lecturer"))
        {
          return Results.Forbid();
        }

        var query = new GetTeacherClassesQuery(teacherId);
        var result = await sender.Send(query);

        return result.Match(
                data => Results.Ok(new
                {
                  status = StatusCodes.Status200OK,
                  message = "Get teacher classes successfully",
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
    .RequireAuthorization("RequireRole_Teacher")
    .WithTags("Program");
  }
}
