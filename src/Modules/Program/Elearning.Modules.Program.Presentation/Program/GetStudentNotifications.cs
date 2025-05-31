using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetStudentNotifications;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Elearning.Common.Infrastructure.Extensions;
namespace Elearning.Modules.Program.Presentation.Program;

// [RequireAuthAttribute("Student")]
internal sealed class GetStudentNotifications : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("student/notifications", async (
            ISender sender,
            HttpContext httpContext,
            int pageSize = 20,
            int pageNumber = 1,
            string? notificationType = null) =>
        {
            // Get student ID from JWT token
            var userIdClaim = httpContext.User.GetUserId();
            if (!Guid.TryParse(userIdClaim, out var actualStudentId))
            {
                return Results.BadRequest("Invalid or missing user ID in token");
            }

            var query = new GetStudentNotificationsQuery
            {
                StudentId = actualStudentId,
                PageSize = Math.Min(pageSize, 100), // Limit max page size
                PageNumber = Math.Max(pageNumber, 1),
                NotificationType = notificationType
            };

            var result = await sender.Send(query);

            return Results.Ok(new
            {
                status = StatusCodes.Status200OK,
                message = "Get student notifications successfully",
                data = result
            });
        })
        .RequireAuthorization();
    }

    private static Guid GetStudentIdFromClaims(HttpContext context)
    {
        Console.WriteLine("GetStudentIdFromClaims", context);
        // Extract student ID from JWT claims
        var userIdClaim = context.User?.FindFirst("sub")?.Value
                     ?? context.User?.FindFirst("userId")?.Value;

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return Guid.Empty;
    }
}
