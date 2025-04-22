using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetProgramByDeparment;

public sealed record GetProgramByDeparmentQuery(Guid department_id) : IQuery<List<GetProgramByDeparmentResponse>>;