using Microsoft.AspNetCore.Routing;
namespace Elearning.Common.Presentation.Endpoints;

public interface IEndpoint
{
  void MapEndpoint(IEndpointRouteBuilder app);
}