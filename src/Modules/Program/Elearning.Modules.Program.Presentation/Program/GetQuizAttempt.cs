using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetQuizAttempt;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher,Student")]
internal sealed class GetQuizAttempt : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("program/quiz-attempts/{attemptId}", async (
            Guid attemptId,
            ISender sender,
            HttpContext httpContext) =>
        {
            var query = new GetQuizAttemptQuery(attemptId);
            var result = await sender.Send(query);

            return result.Match(
                data =>
                {
                    // Check if user has access to this attempt
                    var currentUserId = Guid.Parse(httpContext.User.GetUserId());
                    var userRoles = httpContext.User.GetUserRoles();
                    var isTeacher = userRoles.Contains("Teacher") || userRoles.Contains("Lecturer");
                    var isAdmin = userRoles.Contains("Administrator");

                    if (data.student_id != currentUserId && !isTeacher && !isAdmin)
                    {
                        return Results.Forbid();
                    }

                    return Results.Ok(new
                    {
                        status = StatusCodes.Status200OK,
                        message = "Get quiz attempt successfully",
                        data
                    });
                },
                ApiResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("Program");
    }
}
