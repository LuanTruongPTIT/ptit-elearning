using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Program.GetClassByDepartmentOfTeacher;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Room;

[RequireAuthAttribute("Teacher")]
internal sealed class GetClassByDepartmentOfTeacher : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("room/classes-by-department-of-teacher", async (
        ISender sender, HttpContext context) =>
    {
      var teacherId = context.User.GetUserId();

      Result<List<GetClassByDepartmentOfTeacherResponse>> result =
              await sender.Send(new GetClassByDepartmentOfTeacherQuery((teacherId)));

      return result.Match(
              data => Results.Ok(new
              {
                status = StatusCodes.Status200OK,
                message = "Get classes by department successfully",
                data
              }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization("RequireRole_Teacher")
    .WithTags("Room");
  }
}