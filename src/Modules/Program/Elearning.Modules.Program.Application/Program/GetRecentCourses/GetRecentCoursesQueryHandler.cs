using Elearning.Common.Domain;
using Elearning.Modules.Program.Application.Program.GetStudentCourses;
using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetRecentCourses
{
  public class GetRecentCoursesQueryHandler : IRequestHandler<GetRecentCoursesQuery, GetRecentCoursesResponse>
  {
    private readonly IMediator _mediator;

    public GetRecentCoursesQueryHandler(IMediator mediator)
    {
      _mediator = mediator;
    }

    public async Task<GetRecentCoursesResponse> Handle(GetRecentCoursesQuery request, CancellationToken cancellationToken)
    {
      // Get enrolled courses
      var studentId = request.StudentId ?? Guid.Empty; // Use provided StudentId or default
      var coursesResult = await _mediator.Send(new GetStudentCoursesQuery(studentId), cancellationToken);

      // Handle potential failure
      if (coursesResult.IsFailure)
      {
        // Return empty list if we can't get courses
        return new GetRecentCoursesResponse
        {
          Courses = new List<EnrolledCourseDto>()
        };
      }

      // Get the actual courses from the Result
      var enrolledCourses = coursesResult.Value;

      // Get recent courses (last accessed)
      var recentCourses = enrolledCourses
          .Where(c => c.Status != null) // Filter courses with status
          .OrderByDescending(c => c.enrollment_date) // Order by enrollment date
          .Take(3)
          .Select(c => new EnrolledCourseDto
          {
            Id = c.course_id.ToString(),
            Title = c.course_name,
            Description = c.description,
            Thumbnail = c.thumbnail_url,
            Instructor = c.teacher_name,
            Progress = c.progress_percentage,
            TotalLectures = 0, // Not available in the response
            CompletedLectures = 0, // Not available in the response
            LastAccessed = c.enrollment_date.ToString("o"),
            Category = "Course", // Default category
            Status = c.Status
          })
          .ToList();

      return new GetRecentCoursesResponse
      {
        Courses = recentCourses
      };
    }
  }
}
