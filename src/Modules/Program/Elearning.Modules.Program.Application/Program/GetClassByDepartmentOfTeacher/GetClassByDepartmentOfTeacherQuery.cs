using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetClassByDepartmentOfTeacher;

public sealed record GetClassByDepartmentOfTeacherQuery(string teacher_id) : IQuery<List<GetClassByDepartmentOfTeacherResponse>>;