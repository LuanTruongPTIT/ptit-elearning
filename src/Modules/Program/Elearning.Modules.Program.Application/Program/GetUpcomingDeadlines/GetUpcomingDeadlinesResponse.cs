namespace Elearning.Modules.Program.Application.Program.GetUpcomingDeadlines
{
    public class GetUpcomingDeadlinesResponse
    {
        public List<DeadlineDto> Deadlines { get; set; }
    }

    public class DeadlineDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Course { get; set; }
        public string DueDate { get; set; }
        public string Type { get; set; }
    }
}
