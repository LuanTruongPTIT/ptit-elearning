using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetClassStudents;

public sealed record GetClassStudentsQuery(
    Guid? TeacherId,
    Guid? ClassId,
    int Page = 1,
    int PageSize = 20,
    string? SearchTerm = null,
    string? SortBy = "name",
    string? SortOrder = "asc"
) : IQuery<GetClassStudentsResponse>;
