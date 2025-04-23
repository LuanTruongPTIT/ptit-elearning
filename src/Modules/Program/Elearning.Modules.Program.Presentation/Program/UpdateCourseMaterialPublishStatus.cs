using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.UpdateCourseMaterialPublishStatus;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class UpdateCourseMaterialPublishStatus : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("program/materials/{id}/publish-status", async (
            Guid id,
            UpdateCourseMaterialPublishStatusCommand request,
            ISender sender) =>
        {
            // Set the id from the route parameter
            request = request with { id = id };

            var result = await sender.Send(request);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = $"Update course material publish status to {(request.is_published ? "published" : "unpublished")} successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Teacher")
        .WithTags("Program");
    }
}
