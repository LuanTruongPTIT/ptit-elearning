using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.CreateQuiz;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class CreateQuiz : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("program/assignments/{assignmentId}/quiz", async (
            Guid assignmentId,
            CreateQuizCommand request,
            ISender sender,
            HttpContext httpContext) =>
        {
            // Set the assignment_id from the route parameter
            request = request with { assignment_id = assignmentId };

            // Set the created_by from the authenticated user
            request.created_by = Guid.Parse(httpContext.User.GetUserId());

            var result = await sender.Send(request);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Create quiz successfully",
                    data = new { quiz_id = data }
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Teacher")
        .WithTags("Program");
    }
}
