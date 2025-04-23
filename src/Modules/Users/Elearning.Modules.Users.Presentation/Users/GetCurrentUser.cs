using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Users.Application.Users.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Users.Presentation.Users;


[RequireAuthAttribute()]
internal sealed class GetCurrentUser : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("users/me", async (ISender sender, HttpContext context) =>
    {
      Result<GetCurrentUserResponse> result = await sender.Send(new GetCurrentUserQuery(Guid.Parse(context.User.GetUserId())));

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "User retrieved successfully",
                data = data
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization()
    .WithTags(Tags.Users);
  }
}
