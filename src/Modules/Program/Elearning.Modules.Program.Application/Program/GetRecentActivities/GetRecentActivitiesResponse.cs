namespace Elearning.Modules.Program.Application.Program.GetRecentActivities
{
    public class GetRecentActivitiesResponse
    {
        public List<ActivityDto> Activities { get; set; }
    }

    public class ActivityDto
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Course { get; set; }
        public string Title { get; set; }
        public string Timestamp { get; set; }
        public int? Score { get; set; }
    }
}
