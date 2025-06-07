using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Students.Queries.GetStudents;

public sealed record GetStudentsQuery(
    string? SearchTerm = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetStudentsResponse>;
