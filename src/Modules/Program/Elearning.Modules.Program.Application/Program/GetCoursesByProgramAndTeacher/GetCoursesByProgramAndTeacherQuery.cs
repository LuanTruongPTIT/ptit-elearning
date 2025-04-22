using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetCoursesByProgramAndTeacher;

public sealed record GetCoursesByProgramAndTeacherQuery(
    Guid program_id,
    Guid teacher_id
) : IQuery<List<GetCoursesByProgramAndTeacherResponse>>;

