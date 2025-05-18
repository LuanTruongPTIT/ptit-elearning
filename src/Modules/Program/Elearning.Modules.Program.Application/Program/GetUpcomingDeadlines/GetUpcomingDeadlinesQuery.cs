using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetUpcomingDeadlines
{
  public class GetUpcomingDeadlinesQuery : IRequest<GetUpcomingDeadlinesResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
