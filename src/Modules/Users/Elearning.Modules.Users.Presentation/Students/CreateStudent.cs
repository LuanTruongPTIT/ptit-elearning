using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Users.Application.Students.CreateStudent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Users.Presentation.Students;

[RequireAuthAttribute("Admin")]
internal sealed class CreateStudent : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("users/students", async (CreateStudentCommand request, ISender sender) =>
    {
      Result<string> result = await sender.Send(request);

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Student created successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Admin")
    .WithTags(Tags.Users);
  }
}
