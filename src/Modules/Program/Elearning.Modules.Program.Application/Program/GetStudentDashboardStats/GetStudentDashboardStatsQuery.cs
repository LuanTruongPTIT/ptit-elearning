using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetStudentDashboardStats
{
  public class GetStudentDashboardStatsQuery : IRequest<GetStudentDashboardStatsResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
