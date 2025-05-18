using Elearning.Modules.Program.Application.Program.GetRecentCourses;
using Elearning.Modules.Program.Application.Program.GetUpcomingDeadlines;
using Elearning.Modules.Program.Application.Program.GetRecentActivities;
using Elearning.Modules.Program.Application.Program.GetProgressData;
using Elearning.Modules.Program.Application.Program.GetWeeklyStudyData;
using Elearning.Modules.Program.Application.Program.GetSubjectDistribution;
using Elearning.Modules.Program.Application.Program.GetStudentDashboardStats;
using System.Collections.Generic;

namespace Elearning.Modules.Program.Application.Program.GetStudentDashboard
{
  public class GetStudentDashboardResponse
  {
    public GetStudentDashboardStatsResponse Stats { get; set; } = null!;
    public List<EnrolledCourseDto> RecentCourses { get; set; } = new();
    public List<DeadlineDto> UpcomingDeadlines { get; set; } = new();
    public List<ActivityDto> RecentActivities { get; set; } = new();
    public List<ProgressDataDto> ProgressData { get; set; } = new();
    public List<StudyTimeDataDto> WeeklyStudyData { get; set; } = new();
    public List<SubjectDistributionDto> SubjectDistribution { get; set; } = new();
  }
}
