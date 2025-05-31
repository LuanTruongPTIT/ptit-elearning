using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.StartQuizAttempt;

public sealed record StartQuizAttemptCommand(
    Guid quiz_id,
    string ip_address,
    string user_agent
) : ICommand<Guid>
{
    public Guid student_id { get; set; }
}
