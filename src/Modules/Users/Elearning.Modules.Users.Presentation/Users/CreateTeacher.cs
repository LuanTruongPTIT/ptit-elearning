
using Elearning.Common.Domain;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Modules.Users.Application.Users.CreateTeacher;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;


namespace Elearning.Modules.Users.Presentation.Users;

internal sealed class CreateTeacher : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("users/create-teacher", async (CreateTeacherCommand request, ISender sender) =>
    {
      Result<string> result = await sender.Send(request);
      return result.Match(
        data => Results.Ok(new
        {
          status = StatusCodes.Status200OK,
          message = "Create teacher successfully",
          data = data
        }),
        Common.Presentation.Results.ApiResults.Problem
      );
    })
      .AllowAnonymous()
      .WithTags(Tags.Users);
  }
}