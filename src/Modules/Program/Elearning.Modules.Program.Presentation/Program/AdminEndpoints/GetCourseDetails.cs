using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Courses.Queries.GetCourseById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program.AdminEndpoints;

internal sealed class GetCourseDetails : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/courses/{id}", async (string id, ISender sender) =>
        {
            var query = new GetCourseByIdQuery(id);

            Result<GetCourseByIdResponse> result = await sender.Send(query);

            return result.Match(
                success => Results.Ok(new { status = 200, data = success, message = "Success" }),
                failure => Results.BadRequest(new { status = 400, data = (object?)null, message = failure.ToString() }));
        })
        .RequireAuthorization();
    }
}
