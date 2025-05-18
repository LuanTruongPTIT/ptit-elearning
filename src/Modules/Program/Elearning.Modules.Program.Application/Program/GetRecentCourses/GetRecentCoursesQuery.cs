using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetRecentCourses
{
  public class GetRecentCoursesQuery : IRequest<GetRecentCoursesResponse>
  {
    public Guid? StudentId { get; set; }
  }
}
