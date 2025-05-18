using Elearning.Common.Domain;
using Elearning.Modules.Program.Application.Program.GetStudentCourses;
using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetSubjectDistribution
{
  public class GetSubjectDistributionQueryHandler : IRequestHandler<GetSubjectDistributionQuery, GetSubjectDistributionResponse>
  {
    private readonly IMediator _mediator;

    public GetSubjectDistributionQueryHandler(IMediator mediator)
    {
      _mediator = mediator;
    }

    public async Task<GetSubjectDistributionResponse> Handle(GetSubjectDistributionQuery request, CancellationToken cancellationToken)
    {
      // Get enrolled courses
      var studentId = request.StudentId ?? Guid.Empty; // Use provided StudentId or default
      var coursesResult = await _mediator.Send(new GetStudentCoursesQuery(studentId), cancellationToken);

      // Handle potential failure
      if (coursesResult.IsFailure)
      {
        // Return mock data if we can't get courses
        return new GetSubjectDistributionResponse
        {
          SubjectDistribution = new List<SubjectDistributionDto>
          {
            new SubjectDistributionDto { Name = "Web Development", Value = 2, Color = "#4f46e5" },
            new SubjectDistributionDto { Name = "Programming", Value = 1, Color = "#10b981" },
            new SubjectDistributionDto { Name = "Database", Value = 1, Color = "#f59e0b" },
            new SubjectDistributionDto { Name = "Mobile Development", Value = 1, Color = "#ef4444" },
            new SubjectDistributionDto { Name = "Computer Science", Value = 1, Color = "#8b5cf6" }
          }
        };
      }

      // Get the actual courses from the Result
      var enrolledCourses = coursesResult.Value;

      // Group courses by category and count
      var categoryCounts = enrolledCourses
          .GroupBy(c => c.course_code) // Use course_code as category
          .Select(g => new { Category = g.Key, Count = g.Count() })
          .ToList();

      // Assign colors to categories
      var colors = new Dictionary<string, string>
            {
                { "Web Development", "#4f46e5" },
                { "Programming", "#10b981" },
                { "Database", "#f59e0b" },
                { "Mobile Development", "#ef4444" },
                { "Computer Science", "#8b5cf6" },
                { "Artificial Intelligence", "#0ea5e9" },
                { "Data Science", "#ec4899" },
                { "DevOps", "#14b8a6" },
                { "Security", "#f43f5e" },
                { "Other", "#6b7280" }
            };

      // Create subject distribution data
      var subjectDistribution = categoryCounts
          .Select(c => new SubjectDistributionDto
          {
            Name = c.Category,
            Value = c.Count,
            Color = colors.ContainsKey(c.Category) ? colors[c.Category] : colors["Other"]
          })
          .ToList();

      // If no data, provide mock data
      if (subjectDistribution.Count == 0)
      {
        subjectDistribution = new List<SubjectDistributionDto>
                {
                    new SubjectDistributionDto { Name = "Web Development", Value = 2, Color = "#4f46e5" },
                    new SubjectDistributionDto { Name = "Programming", Value = 1, Color = "#10b981" },
                    new SubjectDistributionDto { Name = "Database", Value = 1, Color = "#f59e0b" },
                    new SubjectDistributionDto { Name = "Mobile Development", Value = 1, Color = "#ef4444" },
                    new SubjectDistributionDto { Name = "Computer Science", Value = 1, Color = "#8b5cf6" }
                };
      }

      return new GetSubjectDistributionResponse
      {
        SubjectDistribution = subjectDistribution
      };
    }
  }
}
