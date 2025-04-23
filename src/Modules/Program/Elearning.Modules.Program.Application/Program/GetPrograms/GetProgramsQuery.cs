using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetPrograms;

public sealed record GetProgramsQuery() : IQuery<List<GetProgramsResponse>>;
