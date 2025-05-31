using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetStudentNotifications;

public class GetStudentNotificationsQuery : IRequest<GetStudentNotificationsResponse>
{
  public Guid StudentId { get; set; }
  public int PageSize { get; set; } = 20;
  public int PageNumber { get; set; } = 1;
  public string? NotificationType { get; set; } // "assignment_created", "deadline_reminder", etc.
}
