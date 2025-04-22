using Elearning.Common.Presentation.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Elearning.Modules.Program.Application.Room.GetListClass;

namespace Elearning.Modules.Program.Presentation.Room;

internal sealed class GetListClass : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("room/classes", async (ISender sender, HttpRequest request) =>
        {
            int page = int.TryParse(request.Query["page"].FirstOrDefault(), out int pageNumber) ? pageNumber : 1;
            int page_size = int.TryParse(request.Query["page_size"].FirstOrDefault(), out int pageSize) ? pageSize : 10;

            var result = await sender.Send(new GetListClassQuery(page, page_size));

            return result.Match(
                data => Results.Ok(new
                {
                    status = StatusCodes.Status200OK,
                    message = "Get list classes successfully",
                    data
                }),
                Common.Presentation.Results.ApiResults.Problem
            );
        })
        .AllowAnonymous()
        .WithTags("Room");
    }
}