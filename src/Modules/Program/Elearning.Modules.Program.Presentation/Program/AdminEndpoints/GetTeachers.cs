using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Teachers.Queries.GetTeachers;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program.AdminEndpoints;

internal sealed class GetTeachers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/teachers", async (
            ISender sender,
            HttpContext httpContext,
            [FromQuery] string? searchTerm,
            [FromQuery] string? status,
            [FromQuery] string? department,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10) =>
        {
            // Check if user is admin
            var userRoles = httpContext.User.GetUserRoles();
            var isAdmin = userRoles.Contains("Administrator");

            if (!isAdmin)
            {
                return Results.Forbid();
            }

            var result = await sender.Send(new GetTeachersQuery(searchTerm, status, department, page, pageSize));

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Get teachers successfully",
                    data
                }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags(Tags.Program);
    }
}
