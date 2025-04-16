using Elearning.Common.Application.Messaging;
namespace Elearning.Modules.Program.Application.Program;

public sealed record ProgramGetCourseDepartmentQuery() : IQuery<List<ProgramGetCourseDepartmentResponse>>;