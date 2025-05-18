using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetProgressData
{
  public class GetProgressDataQuery : IRequest<GetProgressDataResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
