using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Courses.Queries.GetAdminCourses;

public sealed record GetAdminCoursesQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? Department = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetAdminCoursesResponse>;
