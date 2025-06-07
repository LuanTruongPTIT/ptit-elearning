namespace Elearning.Modules.Program.Application.Students.Queries.GetStudents;

public sealed class GetStudentsResponse
{
  public List<StudentDto> Students { get; set; } = [];
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public int CurrentPage { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}

public sealed class StudentDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public DateTime DateOfBirth { get; set; }
  public DateTime EnrollmentDate { get; set; }
  public string Status { get; set; } = string.Empty;
  public int CoursesCount { get; set; }
  public decimal Gpa { get; set; }
  public string Department { get; set; } = string.Empty;
}
