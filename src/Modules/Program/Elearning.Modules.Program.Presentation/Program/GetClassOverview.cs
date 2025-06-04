using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetClassOverview;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class GetClassOverview : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/teacher/class/{classId}/overview", async (
        ISender sender,
        [FromRoute] Guid classId,
        [FromQuery] Guid? teacherId = null) =>
    {
      var query = new GetClassOverviewQuery(teacherId, classId);
      var result = await sender.Send(query);

      return result.Match(
              data => Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = "Get class overview successfully",
              data
            }),
              ApiResults.Problem
          );
    })
    .AllowAnonymous() // Tạm thời cho phép truy cập không cần xác thực
    .WithTags("Teacher");
  }
}
