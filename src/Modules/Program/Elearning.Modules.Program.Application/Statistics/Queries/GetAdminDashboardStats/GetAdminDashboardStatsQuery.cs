using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Statistics.Queries.GetAdminDashboardStats;

public sealed record GetAdminDashboardStatsQuery() : IQuery<AdminDashboardStatsResponse>;
