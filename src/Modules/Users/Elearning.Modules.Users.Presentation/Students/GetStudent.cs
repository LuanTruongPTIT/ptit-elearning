using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Users.Application.Students.GetStudent;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Users.Presentation.Students;

[RequireAuth]
internal sealed class GetStudent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Register both endpoints with the same handler
        app.MapGet("users/students/{studentId}", HandleRequest)
            .RequireAuthorization()
            .WithTags(Tags.Users);

        // Add singular form endpoint for compatibility
        app.MapGet("users/student/{studentId}", HandleRequest)
            .RequireAuthorization()
            .WithTags(Tags.Users);
    }
    
    // Handler function to process the request
    private static async Task<IResult> HandleRequest(
        string studentId,
        ISender sender,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // Get user ID and role from claims
            var userId = context.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var userRole = context.User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            var result = await sender.Send(
                    new GetStudentQuery(studentId, userId, userRole),
                    cancellationToken
                );

            return result.Match(
                    data => Results.Ok(new
                    {
                        status = StatusCodes.Status200OK,
                        message = "Student retrieved successfully",
                        data
                    }),
                    ApiResults.Problem
                );
        }
        catch (Exception ex)
        {
            // Log exception
            Console.WriteLine($"Exception in GetStudent endpoint: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Return error response
            return Results.Problem(
                title: "An unexpected error occurred",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError
            );
        }
    }
}
