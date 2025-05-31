namespace Elearning.Modules.Program.Application.Program.GetUpcomingDeadlines
{
  public class GetUpcomingDeadlinesResponse
  {
    public List<DeadlineDto> Deadlines { get; set; } = new();
  }

  public class DeadlineDto
  {
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Course { get; set; } = string.Empty;
    public string DueDate { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public decimal? MaxScore { get; set; }
    public string? InstructorId { get; set; }
    public bool IsNew { get; set; } = false; // To mark newly created assignments
    public DateTime CreatedAt { get; set; }
  }
}
