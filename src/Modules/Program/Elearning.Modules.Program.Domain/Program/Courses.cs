namespace Elearning.Modules.Program.Domain.Program;

public sealed class Courses
{
  private Courses() { }
  public Guid id { get; private set; }
  public string name { get; private set; }
  public string code { get; private set; }

  private readonly List<ProgramUnit> _programs = new();
  public IReadOnlyCollection<ProgramUnit> Programs => _programs;

  public DateTime created_at { get; private set; }
  public DateTime updated_at { get; private set; }
}
