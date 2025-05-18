using Elearning.Modules.Program.Application.Program.GetRecentCourses;
using Elearning.Modules.Program.Application.Program.GetUpcomingDeadlines;
using Elearning.Modules.Program.Application.Program.GetRecentActivities;
using Elearning.Modules.Program.Application.Program.GetProgressData;
using Elearning.Modules.Program.Application.Program.GetWeeklyStudyData;
using Elearning.Modules.Program.Application.Program.GetSubjectDistribution;
using Elearning.Modules.Program.Application.Program.GetStudentDashboardStats;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Elearning.Modules.Program.Application.Program.GetStudentDashboard
{
  public class GetStudentDashboardQueryHandler : IRequestHandler<GetStudentDashboardQuery, GetStudentDashboardResponse>
  {
    private readonly IMediator _mediator;

    public GetStudentDashboardQueryHandler(IMediator mediator)
    {
      _mediator = mediator;
    }

    public async Task<GetStudentDashboardResponse> Handle(GetStudentDashboardQuery request, CancellationToken cancellationToken)
    {
      // Get all dashboard data in parallel
      var studentId = request.StudentId;

      var statsTask = _mediator.Send(new GetStudentDashboardStatsQuery { StudentId = studentId }, cancellationToken);
      var recentCoursesTask = _mediator.Send(new GetRecentCoursesQuery { StudentId = studentId }, cancellationToken);
      var upcomingDeadlinesTask = _mediator.Send(new GetUpcomingDeadlinesQuery(), cancellationToken);
      var recentActivitiesTask = _mediator.Send(new GetRecentActivitiesQuery(), cancellationToken);
      var progressDataTask = _mediator.Send(new GetProgressDataQuery(), cancellationToken);
      var weeklyStudyDataTask = _mediator.Send(new GetWeeklyStudyDataQuery(), cancellationToken);
      var subjectDistributionTask = _mediator.Send(new GetSubjectDistributionQuery { StudentId = studentId }, cancellationToken);

      // Wait for all tasks to complete
      await Task.WhenAll(
          statsTask,
          recentCoursesTask,
          upcomingDeadlinesTask,
          recentActivitiesTask,
          progressDataTask,
          weeklyStudyDataTask,
          subjectDistributionTask
      );

      // Combine results
      return new GetStudentDashboardResponse
      {
        Stats = await statsTask,
        RecentCourses = (await recentCoursesTask).Courses,
        UpcomingDeadlines = (await upcomingDeadlinesTask).Deadlines,
        RecentActivities = (await recentActivitiesTask).Activities,
        ProgressData = (await progressDataTask).ProgressData,
        WeeklyStudyData = (await weeklyStudyDataTask).WeeklyStudyData,
        SubjectDistribution = (await subjectDistributionTask).SubjectDistribution
      };
    }
  }
}
