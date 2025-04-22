using Elearning.Common.Domain;
using Elearning.Common.Infrastructure.Auth;
using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Users.Application.Teachers.GetListTeacher;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Users.Presentation.Teachers;


// [RequireAuthAttribute("Admin")]
internal sealed class GetListTeacher : IEndpoint
{

  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("users/teachers", async (ISender sender, HttpRequest request, HttpContext context, CancellationToken cancellationToken) =>
    {
      var user = context.User;
      int page = int.TryParse(request.Query["page"].FirstOrDefault(), out int pageNumber) ? pageNumber : 1;
      int page_size = int.TryParse(request.Query["page_size"].FirstOrDefault(), out int pageSize) ? pageSize : 10;
      Console.WriteLine(page_size.ToString());
      Result<List<GetListTeacherResponse>> result = await sender.Send(new GetListTeacherQuery(request.Query["keyword"], page, page_size));

      return result.Match(data =>
      {
        return Results.Ok(new
        {
          status = StatusCodes.Status200OK,
          message = "Get list teacher successfully",
          total = data.Count,
          data
        });
      }, ApiResults.Problem);
    })
    // .RequireAuthorization("RequireRole_Admin")
    .WithTags(Tags.Users);
  }
}
