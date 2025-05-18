using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetStudentDashboard
{
  public class GetStudentDashboardQuery : IRequest<GetStudentDashboardResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
