using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetRecentActivities
{
    public class GetRecentActivitiesQueryHandler : IRequestHandler<GetRecentActivitiesQuery, GetRecentActivitiesResponse>
    {
        public async Task<GetRecentActivitiesResponse> Handle(GetRecentActivitiesQuery request, CancellationToken cancellationToken)
        {
            // Mock data for recent activities
            // In a real application, this would come from a database
            var activities = new List<ActivityDto>
            {
                new ActivityDto
                {
                    Id = "activity-1",
                    Type = "completed_lecture",
                    Course = "Introduction to Web Development",
                    Title = "CSS Flexbox and Grid",
                    Timestamp = DateTime.Now.AddHours(-2).ToString("o")
                },
                new ActivityDto
                {
                    Id = "activity-2",
                    Type = "started_course",
                    Course = "Mobile App Development with React Native",
                    Title = "Getting Started with React Native",
                    Timestamp = DateTime.Now.AddDays(-1).ToString("o")
                },
                new ActivityDto
                {
                    Id = "activity-3",
                    Type = "completed_quiz",
                    Course = "Advanced JavaScript Programming",
                    Title = "Promises and Async/Await Quiz",
                    Timestamp = DateTime.Now.AddDays(-2).ToString("o"),
                    Score = 85
                },
                new ActivityDto
                {
                    Id = "activity-4",
                    Type = "submitted_assignment",
                    Course = "Database Design and SQL",
                    Title = "Database Schema Design",
                    Timestamp = DateTime.Now.AddDays(-3).ToString("o")
                }
            };

            return new GetRecentActivitiesResponse
            {
                Activities = activities
            };
        }
    }
}
