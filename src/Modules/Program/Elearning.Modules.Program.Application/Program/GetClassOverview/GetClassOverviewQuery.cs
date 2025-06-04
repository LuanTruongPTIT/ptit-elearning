using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetClassOverview;

public sealed record GetClassOverviewQuery(Guid? TeacherId, Guid? ClassId) : IQuery<GetClassOverviewResponse>;
