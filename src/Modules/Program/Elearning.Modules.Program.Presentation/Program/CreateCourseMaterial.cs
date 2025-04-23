using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.CreateCourseMaterial;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class CreateCourseMaterial : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("program/courses/{courseId}/materials", async (
            Guid courseId,
            CreateCourseMaterialCommand request,
            ISender sender,
            HttpContext httpContext) =>
        {
            // Set the course_id from the route parameter
            request = request with { course_id = courseId };
            
            // Set the created_by from the authenticated user
            request.created_by = Guid.Parse(httpContext.User.GetUserId());

            var result = await sender.Send(request);

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Create course material successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Teacher")
        .WithTags("Program");
    }
}
