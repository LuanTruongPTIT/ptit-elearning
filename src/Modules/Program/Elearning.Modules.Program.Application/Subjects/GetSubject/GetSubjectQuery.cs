using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Subjects.GetSubject;

public sealed record GetSubjectQuery(
    string SubjectId,
    string? UserId = null,
    string? UserRole = null
) : IQuery<GetSubjectResponse>;
