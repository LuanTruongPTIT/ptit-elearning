using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetProgressData;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Student")]
internal sealed class GetProgressData : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("student/dashboard/progress", async (
        ISender sender,
        HttpContext httpContext) =>
    {
      var userId = Guid.Parse(httpContext.User.GetUserId());
      var query = new GetProgressDataQuery { StudentId = userId };
      var result = await sender.Send(query);

      return Results.Ok(new
      {
        status = StatusCodes.Status200OK,
        message = "Get progress data successfully",
        data = result
      });
    })
    .AllowAnonymous() // Tạm thời cho phép truy cập không cần xác thực
    .WithTags("Student");
  }
}
