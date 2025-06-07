namespace Elearning.Modules.Program.Application.Teachers.Queries.GetTeachers;

public sealed class GetTeachersResponse
{
  public List<TeacherDto> Teachers { get; set; } = [];
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public int CurrentPage { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}

public sealed class TeacherDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public string Department { get; set; } = string.Empty;
  public DateTime JoinDate { get; set; }
  public string Status { get; set; } = string.Empty;
  public int CoursesCount { get; set; }
  public int StudentsCount { get; set; }
  public decimal Rating { get; set; }
}
