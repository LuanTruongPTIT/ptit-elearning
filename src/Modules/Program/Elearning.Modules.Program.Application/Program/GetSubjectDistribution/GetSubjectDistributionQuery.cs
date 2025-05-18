using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetSubjectDistribution
{
  public class GetSubjectDistributionQuery : IRequest<GetSubjectDistributionResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
