using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetQuiz;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher,Student")]
internal sealed class GetQuiz : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("program/quiz/{quizId}", async (
            Guid quizId,
            ISender sender) =>
        {
            var query = new GetQuizQuery(quizId);
            var result = await sender.Send(query);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Get quiz successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags("Program");
    }
}
