using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetLectures;

public sealed record GetLecturesQuery(
    Guid teaching_assign_course_id
) : IQuery<List<GetLecturesResponse>>;
