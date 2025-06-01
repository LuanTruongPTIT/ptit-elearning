using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Users.Application.Students.GetStudents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Users.Presentation.Students;

[RequireAuth]
internal sealed class GetStudents : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    // Map both plural and singular endpoints
    app.MapGet("users/students", HandleRequest)
        .RequireAuthorization()
        .WithTags(Tags.Users);

    app.MapGet("users/student", HandleRequest)
        .RequireAuthorization()
        .WithTags(Tags.Users);
  }

  // Handler function for both endpoints
  private static async Task<IResult> HandleRequest(
      ISender sender,
      HttpRequest request,
      HttpContext httpContext,
      CancellationToken cancellationToken)
  {
    try
    {
      // Get query parameters
      string? keyword = request.Query["keyword"].FirstOrDefault();
      int page = int.TryParse(request.Query["page"].FirstOrDefault(), out int pageNumber) ? pageNumber : 1;
      int pageSize = int.TryParse(request.Query["page_size"].FirstOrDefault(), out int pageSizeNumber) ? pageSizeNumber : 10;

      // Additional filters
      Guid? programId = null;
      if (Guid.TryParse(request.Query["program_id"].FirstOrDefault(), out Guid parsedProgramId))
      {
        programId = parsedProgramId;
      }

      int? accountStatus = null;
      if (int.TryParse(request.Query["account_status"].FirstOrDefault(), out int parsedStatus))
      {
        accountStatus = parsedStatus;
      }

      string? sortBy = request.Query["sort_by"].FirstOrDefault();
      string? sortOrder = request.Query["sort_order"].FirstOrDefault();

      // Get user ID and role from claims
      var userId = Guid.Parse(httpContext.User.GetUserId());
      var userRole = httpContext.User.GetUserRoles().FirstOrDefault();

      Console.WriteLine($"GetStudents endpoint: UserRole={userRole}, UserId={userId}");

      // Send query to handler
      var result = await sender.Send(
          new GetStudentsQuery(
            keyword,
            page,
            pageSize,
            userId,
            userRole,
            programId,
            accountStatus,
            sortBy,
            sortOrder
          ),
          cancellationToken
      );

      // Return appropriate response
      return result.Match(
          data => Results.Ok(new
          {
            status = StatusCodes.Status200OK,
            message = "Students retrieved successfully",
            total = data.TotalCount,
            data = data.Students,
            pagination = new
            {
              total_count = data.TotalCount,
              page = data.Page,
              page_size = data.PageSize,
              total_pages = data.TotalPages,
              has_next_page = data.HasNextPage,
              has_previous_page = data.HasPreviousPage
            }
          }),
          ApiResults.Problem
      );
    }
    catch (Exception ex)
    {
      // Log exception
      Console.WriteLine($"Exception in GetStudents endpoint: {ex.Message}");
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
