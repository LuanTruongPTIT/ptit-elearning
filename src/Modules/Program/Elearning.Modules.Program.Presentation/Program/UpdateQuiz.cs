using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.UpdateQuiz;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class UpdateQuiz : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("program/quiz/{quizId}", async (
            Guid quizId,
            UpdateQuizRequest request,
            ISender sender) =>
        {
            var command = new UpdateQuizCommand(
                quizId,
                request.quiz_title,
                request.quiz_description,
                request.time_limit_minutes,
                request.max_attempts,
                request.shuffle_questions,
                request.shuffle_answers,
                request.show_results_immediately,
                request.show_correct_answers,
                request.passing_score_percentage,
                request.allow_review,
                request.auto_submit_on_timeout
            );

            var result = await sender.Send(command);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Quiz updated successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Teacher")
        .WithTags("Program");
    }
}

public sealed record UpdateQuizRequest(
    string quiz_title,
    string quiz_description,
    int? time_limit_minutes,
    int max_attempts,
    bool shuffle_questions,
    bool shuffle_answers,
    bool show_results_immediately,
    bool show_correct_answers,
    decimal? passing_score_percentage,
    bool allow_review,
    bool auto_submit_on_timeout
);
