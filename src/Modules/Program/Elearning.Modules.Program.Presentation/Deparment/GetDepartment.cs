
using Elearning.Common.Domain;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetDepartment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Deparment;

internal sealed class GetDepartment : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/department", async (ISender sender) =>
    {
      Result<List<GetDepartmentResponse>> result = await sender.Send(new GetDepartmentQuery());

      return result.Match(data =>
      {
        return Results.Ok(new
        {
          status = StatusCodes.Status200OK,
          message = "Get department successfully",
          data
        });
      }, ApiResults.Problem);
    })
    .AllowAnonymous()
    .WithTags(Tags.Program);
  }
}