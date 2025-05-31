using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.SubmitQuizAttempt;
using Elearning.Modules.Program.Application.Program.GetQuizAttempt;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class SubmitQuizAttempt : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("program/quiz-attempts/{attemptId}/submit", async (
            Guid attemptId,
            SubmitQuizAttemptRequest request,
            ISender sender,
            HttpContext httpContext) =>
        {
            // First verify the attempt belongs to the current user
            var getAttemptQuery = new GetQuizAttemptQuery(attemptId);
            var attemptResult = await sender.Send(getAttemptQuery);

            if (attemptResult.IsFailure)
            {
                return ApiResults.Problem(attemptResult);
            }

            var currentUserId = Guid.Parse(httpContext.User.GetUserId());
            if (attemptResult.Value.student_id != currentUserId)
            {
                return Results.Forbid();
            }

            var command = new SubmitQuizAttemptCommand(attemptId, request.force_submit);
            var result = await sender.Send(command);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Quiz attempt submitted successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Student")
        .WithTags("Program");
    }
}

public sealed record SubmitQuizAttemptRequest(
    bool force_submit
);
