using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Room;



public sealed class CreateClassCommand : ICommand<string>
{
  public Guid department_id { get; set; }
  public Guid program_id { get; set; }
  public string className { get; set; }
  public string academicPeriod { get; set; }
  public string status { get; set; }
}