using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentSubmissions;

public sealed record GetAssignmentSubmissionsQuery(
    Guid AssignmentId,
    Guid TeacherId,
    string? Status = null,
    int Page = 1,
    int PageSize = 10
) : IQuery<GetAssignmentSubmissionsResponse>;

public class GetAssignmentSubmissionsResponse
{
  public List<AssignmentSubmissionDto> Submissions { get; set; } = new();
  public AssignmentInfoDto Assignment { get; set; } = new();
  public PaginationDto Pagination { get; set; } = new();
}

public class AssignmentSubmissionDto
{
  public Guid Id { get; set; }
  public Guid StudentId { get; set; }
  public string StudentName { get; set; } = string.Empty;
  public string StudentEmail { get; set; } = string.Empty;
  public string? StudentAvatar { get; set; }
  public string SubmissionType { get; set; } = string.Empty;
  public List<string> FileUrls { get; set; } = new();
  public string? TextContent { get; set; }
  public DateTime SubmittedAt { get; set; }
  public decimal? Grade { get; set; }
  public string? Feedback { get; set; }
  public string Status { get; set; } = string.Empty;
  public bool IsLate { get; set; }
  public TimeSpan? LateDuration { get; set; }
}

public class AssignmentInfoDto
{
  public Guid Id { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public DateTime Deadline { get; set; }
  public decimal MaxScore { get; set; }
  public int TotalSubmissions { get; set; }
  public int GradedSubmissions { get; set; }
  public int PendingSubmissions { get; set; }
}

public class PaginationDto
{
  public int Page { get; set; }
  public int PageSize { get; set; }
  public int TotalCount { get; set; }
  public int TotalPages { get; set; }
  public bool HasNextPage { get; set; }
  public bool HasPreviousPage { get; set; }
}
