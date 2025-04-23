using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetLectures;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class GetLectures : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/teaching-assign-courses/{teachingAssignCourseId}/lectures", async (
        Guid teachingAssignCourseId,
        ISender sender) =>
    {
      var result = await sender.Send(
              new GetLecturesQuery(teachingAssignCourseId)
          );

      return result.Match(
              data => Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = "Get lectures successfully",
              data
            }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Teacher")
    .WithTags("Program");
  }
}
