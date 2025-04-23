using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetProgram;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

internal sealed class GetProgram : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("program/programs/{programId}", async (string programId, ISender sender) =>
        {
            var result = await sender.Send(new GetProgramQuery(programId));
            
            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Program retrieved successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .AllowAnonymous()
        .WithTags(Tags.Program);
    }
}
