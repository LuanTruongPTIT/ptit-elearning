using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetStudentDashboard;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class GetStudentDashboard : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/student/dashboard", async (
        [FromQuery] string? studentId,
        ISender sender,
        HttpContext httpContext) =>
    {
      // If studentId is not provided, try to get it from the current user
      Guid? studentGuid = null;
      if (studentId == null)
      {
        studentGuid = Guid.Parse(httpContext.User.GetUserId());
      }
      else if (Guid.TryParse(studentId, out Guid parsedId))
      {
        studentGuid = parsedId;
      }

      var query = new GetStudentDashboardQuery { StudentId = studentGuid };
      var result = await sender.Send(query);

      return Results.Ok(new
      {
        status = StatusCodes.Status200OK,
        message = "Get student dashboard data successfully",
        data = result
      });
    })
   .RequireAuthorization()
    .WithTags("Student");
  }
}
