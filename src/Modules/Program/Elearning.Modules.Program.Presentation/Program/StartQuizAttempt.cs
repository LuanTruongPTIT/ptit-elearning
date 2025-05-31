using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.StartQuizAttempt;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class StartQuizAttempt : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("program/quiz/{quizId}/start", async (
        Guid quizId,
        ISender sender,
        HttpContext httpContext) =>
    {
      var command = new StartQuizAttemptCommand(
              quizId,
              httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
              httpContext.Request.Headers["User-Agent"].ToString()
          )
      {
        student_id = Guid.Parse(httpContext.User.GetUserId())
      };

      var result = await sender.Send(command);

      return result.Match(
              data => Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = "Quiz attempt started successfully",
              data = new { attempt_id = data }
            }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Student")
    .WithTags("Program");
  }
}
