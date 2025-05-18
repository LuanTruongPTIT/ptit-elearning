using Elearning.Common.Domain;
using Elearning.Modules.Program.Application.Program.GetStudentCourses;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Elearning.Modules.Program.Application.Program.GetStudentDashboardStats
{
  public class GetStudentDashboardStatsQueryHandler : IRequestHandler<GetStudentDashboardStatsQuery, GetStudentDashboardStatsResponse>
  {
    private readonly IMediator _mediator;

    public GetStudentDashboardStatsQueryHandler(IMediator mediator)
    {
      _mediator = mediator;
    }

    public async Task<GetStudentDashboardStatsResponse> Handle(GetStudentDashboardStatsQuery request, CancellationToken cancellationToken)
    {
      // Get enrolled courses
      var studentId = request.StudentId ?? Guid.Empty; // Use provided StudentId or default
      var coursesResult = await _mediator.Send(new GetStudentCoursesQuery(studentId), cancellationToken);

      // Handle potential failure
      if (coursesResult.IsFailure)
      {
        // Return empty stats if we can't get courses
        return new GetStudentDashboardStatsResponse
        {
          TotalCourses = 0,
          CompletedCourses = 0,
          InProgressCourses = 0,
          NotStartedCourses = 0,
          OverallProgress = 0
        };
      }

      // Get the actual courses from the Result
      var enrolledCourses = coursesResult.Value;

      // Map status based on progress
      foreach (var course in enrolledCourses)
      {
        if (course.progress_percentage == 100)
          course.Status = "completed";
        else if (course.progress_percentage > 0)
          course.Status = "in_progress";
        else
          course.Status = "not_started";
      }

      // Calculate statistics
      int totalCourses = enrolledCourses.Count;
      int completedCourses = enrolledCourses.Count(c => c.Status == "completed");
      int inProgressCourses = enrolledCourses.Count(c => c.Status == "in_progress");
      int notStartedCourses = enrolledCourses.Count(c => c.Status == "not_started");

      // Calculate overall progress
      int overallProgress = totalCourses > 0
          ? (int)Math.Round(enrolledCourses.Average(c => c.progress_percentage))
          : 0;

      return new GetStudentDashboardStatsResponse
      {
        TotalCourses = totalCourses,
        CompletedCourses = completedCourses,
        InProgressCourses = inProgressCourses,
        NotStartedCourses = notStartedCourses,
        OverallProgress = overallProgress
      };
    }
  }
}
