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
      await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

      // Query to get upcoming assignments for the student
      const string sql = @"
                SELECT 
                    a.id,
                    a.title,
                    tac.course_name as course,
                    a.deadline as due_date,
                    a.assignment_type as type,
                    a.max_score,
                    a.is_published,
                    a.created_at,
                    tac.instructor_id,
                    CASE 
                        WHEN a.created_at > @recentThreshold THEN true 
                        ELSE false 
                    END as is_new
                FROM programs.table_assignments a
                INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                WHERE a.is_published = true 
                    AND a.deadline > @currentTime
                    AND tac.id IN (
                        -- Get courses that the student is enrolled in
                        -- This would need to be joined with enrollment table
                        -- For now, we'll get all published assignments
                        SELECT DISTINCT teaching_assign_course_id 
                        FROM programs.table_assignments 
                        WHERE is_published = true
                    )
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
        Title = a.title,
        Course = a.course,
        DueDate = ((DateTime)a.due_date).ToString("o"),
        Type = a.type,
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
  }
}
