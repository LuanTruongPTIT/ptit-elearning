using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.SubmitAssignment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class SubmitAssignment : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("student/assignments/{assignmentId}/submit", async (
        Guid assignmentId,
        SubmitAssignmentRequest request,
        ISender sender,
        HttpContext httpContext) =>
    {
      var studentId = Guid.Parse(httpContext.User.GetUserId());
      var command = new SubmitAssignmentCommand(
              assignmentId,
              studentId,
              request.SubmissionType,
              request.FileUrls,
              request.TextContent,
              request.QuizAnswers
          );

      var result = await sender.Send(command);

      return result.Match(
              data => Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = data.Message,
              data = new
              {
                submission_id = data.SubmissionId,
                submitted_at = data.SubmittedAt
              }
            }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Student")
    .WithTags("Student");
  }
}

public sealed record SubmitAssignmentRequest(
    string SubmissionType,
    List<string>? FileUrls,
    string? TextContent,
    Dictionary<string, object>? QuizAnswers
);
