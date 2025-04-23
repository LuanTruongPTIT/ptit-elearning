using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.DeleteCourseMaterial;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class DeleteCourseMaterial : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("program/materials/{id}", async (
            Guid id,
            ISender sender) =>
        {
            var result = await sender.Send(
                new DeleteCourseMaterialCommand(id)
            );

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Delete course material successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Teacher")
        .WithTags("Program");
    }
}
