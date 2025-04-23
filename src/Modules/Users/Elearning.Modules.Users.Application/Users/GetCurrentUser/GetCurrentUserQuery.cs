using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Users.Application.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery(
  Guid user_id
) : IQuery<GetCurrentUserResponse>;
