namespace Elearning.Modules.Program.Application.Courses.Queries.GetAdminCourses;

public sealed class GetAdminCoursesResponse
{
  public List<AdminCourseDto> Courses { get; set; } = [];
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public int CurrentPage { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}

public sealed class AdminCourseDto
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Code { get; set; } = string.Empty;
  public string Instructor { get; set; } = string.Empty;
  public string Department { get; set; } = string.Empty;
  public DateTime StartDate { get; set; }
  public DateTime? EndDate { get; set; }
  public string Status { get; set; } = string.Empty;
  public int StudentsCount { get; set; }
  public int MaxStudents { get; set; }
  public int Duration { get; set; } // in hours
  public decimal Rating { get; set; }
  public decimal CompletionRate { get; set; }
  public string ThumbnailUrl { get; set; } = string.Empty;
}
