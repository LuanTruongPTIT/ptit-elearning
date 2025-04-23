using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Subjects.GetSubject;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Subjects;

[RequireAuth]
internal sealed class GetSubject : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("courses/subjects/{subjectId}", HandleRequest)
        .RequireAuthorization();
    // .WithTags(Tags.Courses);
  }

  // Handler function to process the request
  private static async Task<IResult> HandleRequest(
      string subjectId,
      ISender sender,
      HttpContext context,
      CancellationToken cancellationToken)
  {
    try
    {
      // Get user ID and role from claims
      var userId = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
      var userRole = context.User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

      var result = await sender.Send(
              new GetSubjectQuery(subjectId, userId, userRole),
              cancellationToken
          );

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Subject retrieved successfully",
                data
              }),
              ApiResults.Problem
          );
    }
    catch (Exception ex)
    {
      // Log exception
      Console.WriteLine($"Exception in GetSubject endpoint: {ex.Message}");
      Console.WriteLine($"Stack trace: {ex.StackTrace}");

      // Return error response
      return Results.Problem(
          title: "An unexpected error occurred",
          detail: ex.Message,
          statusCode: StatusCodes.Status500InternalServerError
      );
    }
  }
}
