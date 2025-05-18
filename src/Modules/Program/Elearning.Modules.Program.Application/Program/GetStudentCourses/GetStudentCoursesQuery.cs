using Elearning.Common.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Elearning.Modules.Program.Application.Program.GetStudentCourses;

public sealed record GetStudentCoursesQuery(
    Guid student_id
) : IQuery<List<GetStudentCoursesResponse>>;
