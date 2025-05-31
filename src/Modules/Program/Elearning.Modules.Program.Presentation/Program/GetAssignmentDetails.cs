using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetAssignmentDetails;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class GetAssignmentDetails : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("student/assignments/{assignmentId}", async (
        Guid assignmentId,
        ISender sender,
        HttpContext httpContext) =>
    {
      var studentId = Guid.Parse(httpContext.User.GetUserId());
      var query = new GetAssignmentDetailsQuery(assignmentId, studentId);
      var result = await sender.Send(query);

      return result.Match(
              data => Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = "Get assignment details successfully",
              data
            }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Student")
    .WithTags("Student");
  }
}
