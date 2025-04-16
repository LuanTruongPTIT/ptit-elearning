using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Domain.Program;

public sealed class ProgramUnit
{
  private ProgramUnit() { }
  public Guid id { get; private set; }
  public Guid department_id { get; private set; }
  public string name { get; private set; }
  public string code { get; private set; }

  public Department Department { get; private set; }

  private readonly List<Courses> _courses = new();
  public IReadOnlyCollection<Courses> Courses => _courses;

  public DateTime created_at { get; private set; }
  public DateTime updated_at { get; private set; }
}
