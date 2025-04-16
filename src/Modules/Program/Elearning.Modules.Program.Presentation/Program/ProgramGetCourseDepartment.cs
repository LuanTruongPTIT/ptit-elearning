using Elearning.Common.Domain;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class ProgramGetCourseDepartment : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/course-department", async (ISender sender) =>
    {
      Result<List<ProgramGetCourseDepartmentResponse>> result = await sender.Send(new ProgramGetCourseDepartmentQuery());

      return result.Match(data =>
      {
        return Results.Ok(new
        {
          status = StatusCodes.Status200OK,
          message = "Get course department successfully",
          data
        });
      }, ApiResults.Problem);
    })
    .AllowAnonymous()
    .WithTags(Tags.Program);
  }


}