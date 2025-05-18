namespace Elearning.Modules.Program.Application.Program.GetStudentDashboardStats
{
    public class GetStudentDashboardStatsResponse
    {
        public int TotalCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int NotStartedCourses { get; set; }
        public int OverallProgress { get; set; }
    }
}
