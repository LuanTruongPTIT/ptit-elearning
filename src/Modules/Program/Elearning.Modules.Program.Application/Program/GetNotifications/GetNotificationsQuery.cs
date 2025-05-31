using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetNotifications;

public sealed record GetNotificationsQuery(
    Guid? user_id,
    bool? is_read = null,
    int limit = 20,
    int offset = 0
) : IQuery<GetNotificationsResponse>;
