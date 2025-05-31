using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetNotifications;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher,Student")]
internal sealed class GetNotifications : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("program/notifications", async (
            Guid? userId,
            bool? isRead,
            int limit,
            int offset,
            ISender sender,
            HttpContext httpContext) =>
        {
            // If userId is not provided, use current user's ID
            var currentUserId = Guid.Parse(httpContext.User.GetUserId());
            var userRoles = httpContext.User.GetUserRoles();
            var isTeacher = userRoles.Contains("Teacher") || userRoles.Contains("Lecturer");
            var isAdmin = userRoles.Contains("Administrator");

            // Students can only see their own notifications
            // Teachers and Admins can see any user's notifications
            Guid? targetUserId = null;
            if (userId.HasValue)
            {
                if (isTeacher || isAdmin || userId.Value == currentUserId)
                {
                    targetUserId = userId.Value;
                }
                else
                {
                    return Results.Forbid();
                }
            }
            else
            {
                targetUserId = currentUserId;
            }

            var query = new GetNotificationsQuery(targetUserId, isRead, limit, offset);
            var result = await sender.Send(query);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Get notifications successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("Program");
    }
}
