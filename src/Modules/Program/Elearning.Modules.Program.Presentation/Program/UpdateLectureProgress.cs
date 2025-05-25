using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.UpdateLectureProgress;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class UpdateLectureProgress : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapPost("student/lectures/{lectureId}/progress", async (
        Guid lectureId,
        [FromBody] UpdateLectureProgressRequest request,
        ISender sender,
        HttpContext httpContext) =>
    {
      var userId = Guid.Parse(httpContext.User.GetUserId());
      var command = new UpdateLectureProgressCommand
      {
        LectureId = lectureId,
        StudentId = userId,
        WatchPosition = request.WatchPosition,
        ProgressPercentage = request.ProgressPercentage
      };

      var result = await sender.Send(command);

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Lecture progress updated successfully",
                data = new { success = true }
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Student")
    .WithTags("Student");
  }
}

public sealed record UpdateLectureProgressRequest
{
  public int WatchPosition { get; init; } // Position in seconds
  public int ProgressPercentage { get; init; } // Percentage watched (0-100)
}
