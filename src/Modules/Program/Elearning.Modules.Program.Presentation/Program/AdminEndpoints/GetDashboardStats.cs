using Elearning.Common.Infrastructure.Extensions;
using Elearning.Common.Presentation.Endpoints;
using Elearning.Common.Presentation.Results;
using Elearning.Modules.Program.Application.Statistics.Queries.GetAdminDashboardStats;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Elearning.Modules.Program.Presentation.Program.AdminEndpoints;

internal sealed class GetDashboardStats : IEndpoint
{
  public void MapEndpoint(IEndpointRouteBuilder app)
  {
    app.MapGet("admin/dashboard/statistics", async (
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

      var result = await sender.Send(new GetAdminDashboardStatsQuery());

      return result.Match(
              data => Results.Ok(new
            {
              status = StatusCodes.Status200OK,
              message = "Get admin dashboard statistics successfully",
              data
            }),
              ApiResults.Problem
          );
    })
    .RequireAuthorization()
    .WithTags(Tags.Program);
  }
}
