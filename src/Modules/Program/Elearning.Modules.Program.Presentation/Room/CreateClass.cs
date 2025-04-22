using Elearning.Common.Domain;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Modules.Program.Application.Room;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Room;

internal sealed class CreateClass : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("room/create-class", async (CreateClassCommand request, ISender sender) =>
    {
      Result<string> result = await sender.Send(request);
      return result.Match(
        data => Results.Ok(new
        {
          status = StatusCodes.Status200OK,
          message = "Create class successfully",
          data = data
        }),
        Common.Presentation.Results.ApiResults.Problem
      );
    })
      .AllowAnonymous();
    // .WithTags(Tags.Room);
  }
}