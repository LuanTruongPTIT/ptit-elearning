using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetUpcomingDeadlines
{
    public class GetUpcomingDeadlinesQueryHandler : IRequestHandler<GetUpcomingDeadlinesQuery, GetUpcomingDeadlinesResponse>
    {
        public async Task<GetUpcomingDeadlinesResponse> Handle(GetUpcomingDeadlinesQuery request, CancellationToken cancellationToken)
        {
            // Mock data for upcoming deadlines
            // In a real application, this would come from a database
            var deadlines = new List<DeadlineDto>
            {
                new DeadlineDto
                {
                    Id = "deadline-1",
                    Title = "JavaScript Final Project",
                    Course = "Advanced JavaScript Programming",
                    DueDate = DateTime.Now.AddDays(2).ToString("o"),
                    Type = "assignment"
                },
                new DeadlineDto
                {
                    Id = "deadline-2",
                    Title = "Database Design Quiz",
                    Course = "Database Design and SQL",
                    DueDate = DateTime.Now.AddDays(5).ToString("o"),
                    Type = "quiz"
                },
                new DeadlineDto
                {
                    Id = "deadline-3",
                    Title = "React Native App Submission",
                    Course = "Mobile App Development with React Native",
                    DueDate = DateTime.Now.AddDays(7).ToString("o"),
                    Type = "project"
                }
            };

            return new GetUpcomingDeadlinesResponse
            {
                Deadlines = deadlines
            };
        }
    }
}
