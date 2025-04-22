using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetProgramByDeparment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;


internal sealed class GetProgramByDeparment : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/get-program-by-department", async (ISender sender, [FromQuery(Name = "department_id")] Guid departmentId) =>
        {
          var result = await sender.Send(new GetProgramByDeparmentQuery(departmentId));

          return result.Match(data =>
          {
            return Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = "Get program by department successfully",
              data
            });
          }, ApiResults.Problem);
        })
    .AllowAnonymous()
    .WithTags(Tags.Program);
  }
}
