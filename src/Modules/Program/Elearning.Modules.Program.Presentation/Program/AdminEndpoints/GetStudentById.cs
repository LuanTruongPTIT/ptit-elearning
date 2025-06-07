using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Students.Queries.GetStudentById;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program.AdminEndpoints;

internal sealed class GetStudentById : IEndpoint
{
    public async void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("admin/students/{studentId:guid}", async (
            Guid studentId,
            ISender sender,
            HttpContext httpContext) =>
        {
            // Check if user is admin
            var userRoles = httpContext.User.GetUserRoles();
            var isAdmin = userRoles.Contains("Administrator");

            if (!isAdmin)
            {
                return Results.Forbid();
            }

            var result = await sender.Send(new GetStudentByIdQuery(studentId.ToString()));

            return result.Match(
                data => Results.Ok(new
                  {
                      status = StatusCodes.Status200OK,
                      message = "Get student details successfully",
                      data
                  }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization()
        .WithTags(Tags.Program);
    }
}
