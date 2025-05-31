using Elearning.Common.Domain;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.DownloadAssignmentFile;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System.Security.Claims;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class DownloadAssignmentFile : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("student/assignments/{assignmentId}/download", async (
        Guid assignmentId,
        string fileUrl,
        ClaimsPrincipal user,
        ISender sender) =>
    {
      var studentIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if (!Guid.TryParse(studentIdClaim, out var studentId))
      {
        return Results.Unauthorized();
      }

      var query = new DownloadAssignmentFileQuery(assignmentId, fileUrl, studentId);

      Result<DownloadAssignmentFileResponse> result = await sender.Send(query);

      if (result.IsFailure)
      {
        return result.Error.Code switch
        {
          "Assignment.AccessDenied" => Results.Forbid(),
          "File.NotFound" => Results.NotFound(result.Error.Description),
          _ => Results.BadRequest(result.Error.Description)
        };
      }

      var response = result.Value;

      return Results.File(
              response.FileContent,
              response.ContentType,
              response.FileName
          );
    })
    .RequireAuthorization()
    .WithTags(Tags.Program)
    .WithName("DownloadAssignmentFile")
    .WithSummary("Download assignment attachment file")
    .WithDescription("Download a file attached to an assignment. Student must have access to the assignment.");
  }
}
