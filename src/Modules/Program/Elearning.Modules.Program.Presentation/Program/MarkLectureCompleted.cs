using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.MarkLectureCompleted;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class MarkLectureCompleted : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("student/lectures/{lectureId}/complete", async (
            Guid lectureId,
            ISender sender,
            HttpContext httpContext) =>
        {
            var userId = Guid.Parse(httpContext.User.GetUserId());
            var command = new MarkLectureCompletedCommand
            {
                LectureId = lectureId,
                StudentId = userId
            };

            var result = await sender.Send(command);

            return result.Match(
                data => Results.Ok(new
                  {
                      status = StatusCodes.Status200OK,
                      message = "Lecture marked as completed successfully",
                      data = new { success = true }
                  }),
                ApiResults.Problem
            );
        })
        .RequireAuthorization("RequireRole_Student")
        .WithTags("Student");
    }
}
