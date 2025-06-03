using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetRecentActivities;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class GetStudentDashboardActivities : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/student/dashboard/activities", async (
        ISender sender,
        HttpContext httpContext,
        [FromQuery] string? user_id = null,
        [FromQuery] int limit = 10,
        [FromQuery] int offset = 0) =>
    {
      Guid? userId = null;
      if (!string.IsNullOrEmpty(user_id) && Guid.TryParse(user_id, out var parsedUserId))
      {
        userId = parsedUserId;
      }

      var query = new GetRecentActivitiesQuery(userId, limit, offset);
      var result = await sender.Send(query);

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Get student dashboard activities successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .AllowAnonymous() // Tạm thời cho phép truy cập không cần xác thực
    .WithTags("Student");
  }
}
