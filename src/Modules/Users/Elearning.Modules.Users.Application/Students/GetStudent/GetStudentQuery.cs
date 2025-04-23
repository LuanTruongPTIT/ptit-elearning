using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Users.Application.Students.GetStudent;

public sealed record GetStudentQuery(
    string StudentId,
    string? UserId = null,
    string? UserRole = null
) : IQuery<GetStudentResponse>;
