namespace Elearning.Modules.Program.Application.Students.Queries.GetStudentById;

public sealed class StudentDetailDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Email { get; set; } = string.Empty;
  public string PhoneNumber { get; set; } = string.Empty;
  public DateTime DateOfBirth { get; set; }
  public DateTime EnrollmentDate { get; set; }
  public string Status { get; set; } = string.Empty;
  public string Department { get; set; } = string.Empty;
  public string Program { get; set; } = string.Empty;
  public decimal Gpa { get; set; }
  public int TotalCourses { get; set; }
  public int CompletedCourses { get; set; }
  public int InProgressCourses { get; set; }
  public List<StudentCourseDto> Courses { get; set; } = [];
}

public sealed class StudentCourseDto
{
  public Guid CourseId { get; set; }
  public string CourseName { get; set; } = string.Empty;
  public string CourseCode { get; set; } = string.Empty;
  public string TeacherName { get; set; } = string.Empty;
  public int ProgressPercentage { get; set; }
  public string Status { get; set; } = string.Empty;
  public DateTime EnrollmentDate { get; set; }
  public DateTime? LastAccessed { get; set; }
}
