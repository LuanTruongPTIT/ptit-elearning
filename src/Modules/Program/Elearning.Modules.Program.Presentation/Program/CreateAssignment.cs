using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.CreateAssignment;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class CreateAssignment : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("program/assignments", async (
        CreateAssignmentRequest request,
        ISender sender,
        HttpContext httpContext) =>
    {
      var command = new CreateAssignmentCommand(
              request.course_id,
              request.title,
              request.description,
              request.deadline,
              request.assignment_type,
              request.show_answers,
              request.time_limit,
              request.attachments,
              request.max_score,
              request.is_published
          )
      {
        created_by = Guid.Parse(httpContext.User.GetUserId())
      };

      var result = await sender.Send(command);

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status201Created,
                message = data.message,
                data = new { assignment_id = data.assignment_id }
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Teacher")
    .WithTags("Program");
  }
}

public sealed record CreateAssignmentRequest(
    Guid course_id,
    string title,
    string? description,
    DateTime deadline,
    string assignment_type, // 'upload', 'quiz', 'both'
    bool show_answers,
    int? time_limit,
    List<string>? attachments,
    decimal max_score,
    bool is_published
);
