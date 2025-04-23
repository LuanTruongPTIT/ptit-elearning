using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Subjects.GetSubjects;

public sealed record GetSubjectsQuery(
    string? Keyword = null,
    int? Page = null,
    int? PageSize = null,
    Guid? UserId = null,
    string? UserRole = null
) : IQuery<List<GetSubjectsResponse>>;
