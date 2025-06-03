using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using MediatR;

namespace Elearning.Modules.Program.Application.Program.GetUpcomingDeadlines
{
    public class GetUpcomingDeadlinesQueryHandler : IRequestHandler<GetUpcomingDeadlinesQuery, GetUpcomingDeadlinesResponse>
    {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        public GetUpcomingDeadlinesQueryHandler(IDbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<GetUpcomingDeadlinesResponse> Handle(GetUpcomingDeadlinesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

                // Simplified query to get upcoming assignments (for all students for now)
                const string sql = @"
                  SELECT 
                      a.id,
                      a.title,
                      COALESCE(tac.course_class_name, 'Unknown Course') as course,
                      a.deadline as due_date,
                      COALESCE(a.assignment_type, 'assignment') as type,
                      a.max_score,
                      a.is_published,
                      a.created_at,
                      tac.teacher_id as instructor_id,
                      CASE 
                          WHEN a.created_at > @recentThreshold THEN true 
                          ELSE false 
                      END as is_new
                  FROM programs.table_assignments a
                  LEFT JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                  WHERE a.is_published = true 
                      AND a.deadline > @currentTime
                  ORDER BY a.deadline ASC
                  LIMIT 10";

                var assignments = await connection.QueryAsync<dynamic>(sql, new
                {
                    currentTime = DateTime.UtcNow,
                    recentThreshold = DateTime.UtcNow.AddHours(-24), // Mark as new if created in last 24 hours
                    studentId = request.StudentId
                });

                var deadlines = assignments.Select(a => new DeadlineDto
                {
                    Id = a.id.ToString(),
                    Title = a.title ?? "Untitled Assignment",
                    Course = a.course ?? "Unknown Course",
                    DueDate = ((DateTime)a.due_date).ToString("o"),
                    Type = a.type ?? "assignment",
                    MaxScore = a.max_score,
                    InstructorId = a.instructor_id?.ToString(),
                    IsNew = a.is_new,
                    CreatedAt = a.created_at
                }).ToList();

                return new GetUpcomingDeadlinesResponse
                {
                    Deadlines = deadlines
                };
            }
            catch (Exception ex)
            {
                // Log error and return empty list instead of throwing
                Console.WriteLine($"Error in GetUpcomingDeadlinesQueryHandler: {ex.Message}");

                return new GetUpcomingDeadlinesResponse
                {
                    Deadlines = new List<DeadlineDto>()
                };
            }
        }
    }
}
