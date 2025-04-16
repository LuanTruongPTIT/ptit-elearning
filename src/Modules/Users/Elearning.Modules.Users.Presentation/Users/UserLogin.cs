using Elearning.Common.Domain;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Modules.Users.Application.Users.UserLogin;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Users.Presentation.Users;

internal sealed class UserLogin : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("users/login", async (Request request, ISender sender) =>
    {
      Result<UserLoginResponse> result = await sender.Send(new UserLoginCommand(
                                                           request.email, request.password));
      return result.Match(
        data => Results.Ok(new
        {
          status = StatusCodes.Status200OK,
          message = "Login successfully",
          data = data
        }),
        Common.Presentation.Results.ApiResults.Problem
      );
    })
      .AllowAnonymous()
      .WithTags(Tags.Users);
  }
}

internal sealed record Request(string email, string password);