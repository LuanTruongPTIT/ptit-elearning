using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetAssignmentDetailsForTeacher;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class GetAssignmentDetailsForTeacher : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("teacher/assignments/{assignmentId}/details", async (
            Guid assignmentId,
            ISender sender,
            HttpContext httpContext) =>
        {
            try
            {
                var teacherId = Guid.Parse(httpContext.User.GetUserId());
                var userRoles = httpContext.User.GetUserRoles();

                // Check if user is teacher
                if (!userRoles.Contains("Teacher") && !userRoles.Contains("Lecturer"))
                {
                    return Results.Forbid();
                }

                var query = new GetAssignmentDetailsForTeacherQuery(assignmentId, teacherId);
                var result = await sender.Send(query);

                return result.Match(
                    data => Results.Ok(new
                    {
                        status = StatusCodes.Status200OK,
                        message = "Assignment details retrieved successfully",
                        data
                    }),
                    ApiResults.Problem
                );
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "An unexpected error occurred",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError
                );
            }
        })
        .RequireAuthorization()
        .WithTags(Tags.Program);
    }
}
