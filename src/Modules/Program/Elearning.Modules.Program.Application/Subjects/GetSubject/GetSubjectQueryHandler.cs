using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Subjects.GetSubject;

internal sealed class GetSubjectQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetSubjectQuery, GetSubjectResponse>
{
    public async Task<Result<GetSubjectResponse>> Handle(GetSubjectQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
            
            var subjectId = request.SubjectId;
            var userRole = request.UserRole;
            var userId = request.UserId;
            
            // Check if user role and ID are provided
            if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userId))
            {
                return Result.Failure<GetSubjectResponse>(
                    new Error("GetSubject.Unauthorized", "You don't have permission to view this subject", ErrorType.Authorization));
            }
            
            string sql;
            object parameters;
            
            if (userRole == "Administrator")
            {
                // Admin can see any subject
                sql = @"
                    SELECT 
                        c.id,
                        c.code,
                        c.name,
                        d.id as department_id,
                        d.name as department_name,
                        c.credits,
                        c.description,
                        c.is_active,
                        c.created_at,
                        c.updated_at
                    FROM 
                        programs.table_courses c
                    LEFT JOIN 
                        programs.table_program_courses pc ON c.id = pc.course_id
                    LEFT JOIN 
                        programs.table_programs p ON pc.program_id = p.id
                    LEFT JOIN 
                        programs.table_departments d ON p.department_id = d.id
                    WHERE 
                        c.id = @SubjectId
                ";
                
                parameters = new
                {
                    SubjectId = Guid.Parse(subjectId)
                };
            }
            else if (userRole == "Lecturer" || userRole == "Teacher")
            {
                // Teacher can only see subjects they teach
                sql = @"
                    SELECT 
                        c.id,
                        c.code,
                        c.name,
                        d.id as department_id,
                        d.name as department_name,
                        c.credits,
                        c.description,
                        c.is_active,
                        c.created_at,
                        c.updated_at
                    FROM 
                        programs.table_courses c
                    JOIN 
                        programs.table_teaching_assign_courses tac ON c.id = tac.course_id
                    LEFT JOIN 
                        programs.table_program_courses pc ON c.id = pc.course_id
                    LEFT JOIN 
                        programs.table_programs p ON pc.program_id = p.id
                    LEFT JOIN 
                        programs.table_departments d ON p.department_id = d.id
                    WHERE 
                        c.id = @SubjectId
                        AND tac.teacher_id = @TeacherId
                ";
                
                parameters = new
                {
                    SubjectId = Guid.Parse(subjectId),
                    TeacherId = Guid.Parse(userId)
                };
            }
            else
            {
                // Other roles can't see subjects
                return Result.Failure<GetSubjectResponse>(
                    new Error("GetSubject.Unauthorized", "You don't have permission to view this subject", ErrorType.Authorization));
            }
            
            var subject = await connection.QueryFirstOrDefaultAsync<GetSubjectResponse>(sql, parameters);
            
            if (subject == null)
            {
                return Result.Failure<GetSubjectResponse>(
                    new Error("GetSubject.NotFound", $"Subject with ID {request.SubjectId} not found", ErrorType.NotFound));
            }
            
            return Result.Success(subject);
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error in GetSubjectQueryHandler: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            
            // Return failure result
            return Result.Failure<GetSubjectResponse>(
                new Error("GetSubject.Error", $"An error occurred while retrieving subject: {ex.Message}", ErrorType.Problem));
        }
    }
}
