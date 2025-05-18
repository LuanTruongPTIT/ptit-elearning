using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetRecentActivities
{
  public class GetRecentActivitiesQuery : IRequest<GetRecentActivitiesResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
