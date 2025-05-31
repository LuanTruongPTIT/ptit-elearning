using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.SubmitQuizResponse;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class SubmitQuizResponse : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("program/quiz-attempts/{attemptId}/responses", async (
            Guid attemptId,
            SubmitQuizResponseRequest request,
            ISender sender) =>
        {
            var command = new SubmitQuizResponseCommand(
                attemptId,
                request.question_id,
                request.selected_answer_ids,
                request.text_response,
                request.time_spent_seconds
            );

            var result = await sender.Send(command);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Response submitted successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Student")
        .WithTags("Program");
    }
}

public sealed record SubmitQuizResponseRequest(
    Guid question_id,
    List<Guid> selected_answer_ids,
    string text_response,
    int? time_spent_seconds
);
