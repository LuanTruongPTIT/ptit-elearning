using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetWeeklyStudyData
{
  public class GetWeeklyStudyDataQuery : IRequest<GetWeeklyStudyDataResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
