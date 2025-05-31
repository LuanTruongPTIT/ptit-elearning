using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetRecentActivities;

public sealed record GetRecentActivitiesQuery(
    Guid? user_id,
    int limit = 20,
    int offset = 0
) : IQuery<GetRecentActivitiesResponse>;
