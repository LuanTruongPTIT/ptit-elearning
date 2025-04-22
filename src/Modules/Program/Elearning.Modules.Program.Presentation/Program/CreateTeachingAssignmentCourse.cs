using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.CreateTeachingAssignmentCourse;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class CreateTeachingAssignmentCourse : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("program/teaching-assignment-course", async (
        CreateTeachingAssignmentCourseCommand request,
        ISender sender, HttpContext httpContext) =>
    {
      request.teacher_id = httpContext.User.GetUserId();
      var result = await sender.Send(request);

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Create teaching assignment course successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Teacher")
    .WithTags("Program");
  }
}