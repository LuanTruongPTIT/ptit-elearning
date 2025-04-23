using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.DeleteLecture;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class DeleteLecture : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("program/lectures/{id}", async (
            Guid id,
            ISender sender) =>
        {
            var result = await sender.Send(
                new DeleteLectureCommand(id)
            );

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Delete lecture successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Teacher")
        .WithTags("Program");
    }
}
