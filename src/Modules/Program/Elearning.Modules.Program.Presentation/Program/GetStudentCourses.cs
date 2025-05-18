using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetStudentCourses;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class GetStudentCourses : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("api/student/courses", async (
        [FromQuery] Guid studentId,
        ISender sender) =>
    {
      var result = await sender.Send(new GetStudentCoursesQuery(studentId));

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Get student courses successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .AllowAnonymous()
    .WithTags(Tags.Program);
  }
}
