using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetPrograms;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class GetPrograms : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("program/programs", async (ISender sender) =>
        {
            var result = await sender.Send(new GetProgramsQuery());
            
            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Programs retrieved successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .AllowAnonymous()
        .WithTags(Tags.Program);
    }
}
