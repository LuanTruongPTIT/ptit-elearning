using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Users.Application.Students.GetStudents;

public sealed record GetStudentsQuery(
    string? Keyword = null,
    int? Page = null,
    int? PageSize = null,
    Guid? UserId = null,
    string? UserRole = null,
    Guid? ProgramId = null,
    int? AccountStatus = null,
    string? SortBy = null,
    string? SortOrder = null
) : IQuery<GetStudentsWithPaginationResponse>;
