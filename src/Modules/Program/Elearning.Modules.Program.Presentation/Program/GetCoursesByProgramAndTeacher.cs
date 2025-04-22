using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetCoursesByProgramAndTeacher;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute()]
internal sealed class GetCoursesByProgramAndTeacher : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/{programId}/courses", async (
        Guid programId,
        ISender sender,
        HttpContext context) =>
    {
      var teacherId = context.User.GetUserId();

      var result = await sender.Send(
              new GetCoursesByProgramAndTeacherQuery(programId, Guid.Parse(teacherId))
          );

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Get courses by program successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Teacher")
    .WithTags("Program");
  }
}