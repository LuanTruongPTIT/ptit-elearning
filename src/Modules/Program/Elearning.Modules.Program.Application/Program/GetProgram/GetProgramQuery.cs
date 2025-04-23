using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetProgram;

public sealed record GetProgramQuery(string programId) : IQuery<GetProgramResponse>;
