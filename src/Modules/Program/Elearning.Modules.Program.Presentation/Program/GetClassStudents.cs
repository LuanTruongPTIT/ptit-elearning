using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetClassStudents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program;

[RequireAuthAttribute("Teacher")]
internal sealed class GetClassStudents : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("program/teacher/class/{classId}/students", async (
        ISender sender,
        [FromRoute] Guid classId,
        [FromQuery] Guid? teacherId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortOrder = "asc") =>
    {
      var query = new GetClassStudentsQuery(
              teacherId,
              classId,
              page,
              pageSize,
              searchTerm,
              sortBy,
              sortOrder);
      var result = await sender.Send(query);

      return result.Match(
              data => Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = "Get class students successfully",
              data
            }),
              ApiResults.Problem
          );
    })
    .AllowAnonymous() // Tạm thời cho phép truy cập không cần xác thực
    .WithTags("Teacher");
  }
}
