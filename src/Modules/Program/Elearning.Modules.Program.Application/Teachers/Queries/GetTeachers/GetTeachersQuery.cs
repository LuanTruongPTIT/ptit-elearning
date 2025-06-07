using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Teachers.Queries.GetTeachers;

public sealed record GetTeachersQuery(
    string? SearchTerm = null,
    string? Status = null,
    string? Department = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetTeachersResponse>;
