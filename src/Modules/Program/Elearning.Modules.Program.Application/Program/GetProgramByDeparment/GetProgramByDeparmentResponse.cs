namespace Elearning.Modules.Program.Application.Program.GetProgramByDeparment;

public sealed class GetProgramByDeparmentResponse
{
  public Guid id { get; set; }
  public string name { get; set; }
  public string code { get; set; }
  public Guid department_id { get; set; }
}
