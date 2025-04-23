using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Subjects.GetSubjects;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Subjects;

[RequireAuth]
internal sealed class GetSubjects : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    // Map endpoint for all subjects (admin)
    app.MapGet("courses/subjects", HandleRequest)
        .RequireAuthorization();
    // .WithTags(Tags.Courses); 

    // Map endpoint for teacher subjects
    app.MapGet("courses/teacher/subjects", HandleRequest)
        .RequireAuthorization();
    // .WithTags(Tags.Courses);
  }

  // Handler function for both endpoints
  private static async Task<IResult> HandleRequest(
      ISender sender,
      HttpRequest request,
      HttpContext context,
      CancellationToken cancellationToken)
  {
    try
    {
      // Get query parameters
      string? keyword = request.Query["keyword"].FirstOrDefault();
      int page = int.TryParse(request.Query["page"].FirstOrDefault(), out int pageNumber) ? pageNumber : 1;
      int pageSize = int.TryParse(request.Query["page_size"].FirstOrDefault(), out int pageSizeNumber) ? pageSizeNumber : 10;

      // // Get user ID and role from claims
      // var userId = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
      // var userRole = context.User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
      var userId = Guid.Parse(context.User.GetUserId());
      var userRole = context.User.GetUserRoles().FirstOrDefault();
      // Send query to handler
      var result = await sender.Send(
          new GetSubjectsQuery(keyword, page, pageSize, userId, userRole),
          cancellationToken
      );

      // Return appropriate response
      return result.Match(
          data => Results.Ok(new
          {
            status = StatusCodes.Status200OK,
            message = "Subjects retrieved successfully",
            total = data.Count,
            data
          }),
          ApiResults.Problem
      );
    }
    catch (Exception ex)
    {
      // Log exception
      Console.WriteLine($"Exception in GetSubjects endpoint: {ex.Message}");
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
