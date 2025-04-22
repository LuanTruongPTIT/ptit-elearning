using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.CreateTeachingAssignmentCourse;

public sealed record CreateTeachingAssignmentCourseCommand(
    string course_name,
    string description,
    string class_id,
    string course_id,
    DateTime start_date,
    DateTime end_date,
    string thumbnail_url
) : ICommand<string>
{
  public string teacher_id { get; set; } = string.Empty;
}
