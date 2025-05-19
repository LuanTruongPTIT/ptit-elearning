using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetStudentCourses;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class GetStudentDashboardProgress : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("api/student/dashboard/progress", async (
        [FromQuery] Guid? studentId,
        ISender sender,
        HttpContext httpContext) =>
    {
      // If studentId is not provided, try to get it from the current user
      if (studentId == null)
      {
        var userId = Guid.Parse(httpContext.User.GetUserId());
        studentId = userId;
      }

      var result = await sender.Send(new GetStudentCoursesQuery(studentId.Value));

      return result.Match(
          data => Results.Ok(new
          {
            status = StatusCodes.Status200OK,
            message = "Get student dashboard progress successfully",
            data = new
            {
              courses = data,
              totalCourses = data.Count,
              completedCourses = data.Count(c => c.status == "completed"),
              inProgressCourses = data.Count(c => c.status == "in_progress"),
              notStartedCourses = data.Count(c => c.status == "not_started"),
              overallProgress = data.Count > 0 ? (int)Math.Round(data.Average(c => c.progress_percentage)) : 0
            }
          }),
          ApiResults.Problem
      );
    })
    .AllowAnonymous() // Tạm thời cho phép truy cập không cần xác thực
    .WithTags("Student");
  }
}
