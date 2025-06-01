using System.Data.Common;
using System.Text.Json;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentDetailsForTeacher;

internal sealed class GetAssignmentDetailsForTeacherQueryHandler : IQueryHandler<GetAssignmentDetailsForTeacherQuery, GetAssignmentDetailsForTeacherResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetAssignmentDetailsForTeacherQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<GetAssignmentDetailsForTeacherResponse>> Handle(GetAssignmentDetailsForTeacherQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

        // Verify teacher has access to this assignment
        const string accessCheckSql = @"
            SELECT COUNT(*)
            FROM programs.table_assignments a
            INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
            WHERE a.id = @AssignmentId AND tac.teacher_id = @TeacherId";

        var hasAccess = await connection.QuerySingleAsync<int>(accessCheckSql, new
        {
            AssignmentId = request.AssignmentId,
            TeacherId = request.TeacherId
        });

        if (hasAccess == 0)
        {
            return Result.Failure<GetAssignmentDetailsForTeacherResponse>(
                Error.NotFound("Assignment.NotFound", "Assignment not found or access denied"));
        }

        // Get assignment details with statistics
        const string assignmentSql = @"
            SELECT 
                a.id,
                a.title,
                a.description,
                a.deadline,
                a.assignment_type,
                a.show_answers,
                a.time_limit_minutes,
                a.attachment_urls,
                a.max_score,
                a.is_published,
                a.created_at,
                a.updated_at,
                tac.course_class_name as course_name,
                c.class_name,
                COUNT(DISTINCT sp.student_id) as total_students,
                COUNT(DISTINCT asub.id) as submissions_count,
                COUNT(DISTINCT CASE WHEN asub.grade IS NOT NULL THEN asub.id END) as graded_submissions,
                COUNT(DISTINCT CASE WHEN asub.grade IS NULL THEN asub.id END) as pending_submissions,
                AVG(asub.grade) as average_grade
            FROM programs.table_assignments a
            INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
            INNER JOIN programs.classes c ON c.id = tac.class_id
            LEFT JOIN programs.table_student_programs sp ON sp.program_id = c.program_id
            LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
            WHERE a.id = @AssignmentId
            GROUP BY a.id, a.title, a.description, a.deadline, a.assignment_type, 
                     a.show_answers, a.time_limit_minutes, a.attachment_urls, 
                     a.max_score, a.is_published, a.created_at, a.updated_at,
                     tac.course_class_name, c.class_name";

        var assignmentResult = await connection.QueryFirstOrDefaultAsync(assignmentSql, new
        {
            AssignmentId = request.AssignmentId
        });

        if (assignmentResult == null)
        {
            return Result.Failure<GetAssignmentDetailsForTeacherResponse>(
                Error.NotFound("Assignment.NotFound", "Assignment not found"));
        }

        // Get recent submissions (last 5)
        const string recentSubmissionsSql = @"
            SELECT 
                asub.id,
                u.full_name as student_name,
                u.email as student_email,
                asub.submitted_at,
                asub.grade,
                COALESCE(asub.status, 'submitted') as status,
                CASE WHEN asub.submitted_at > a.deadline THEN true ELSE false END as is_late
            FROM programs.table_assignment_submissions asub
            INNER JOIN users.table_users u ON asub.student_id = u.id
            INNER JOIN programs.table_assignments a ON asub.assignment_id = a.id
            WHERE asub.assignment_id = @AssignmentId
            ORDER BY asub.submitted_at DESC
            LIMIT 5";

        var recentSubmissions = await connection.QueryAsync<dynamic>(recentSubmissionsSql, new
        {
            AssignmentId = request.AssignmentId
        });

        // Parse attachment URLs
        List<string>? attachmentUrls = ParseAttachmentUrls(assignmentResult.attachment_urls);

        // Map recent submissions
        var recentSubmissionsList = recentSubmissions.Select(sub => new RecentSubmissionInfo(
            (Guid)sub.id,
            (string)sub.student_name,
            (string)sub.student_email,
            (DateTime)sub.submitted_at,
            sub.grade != null ? (decimal?)sub.grade : null,
            (string)(sub.status ?? "submitted"),
            (bool)sub.is_late
        )).ToList();

        var response = new GetAssignmentDetailsForTeacherResponse(
            (Guid)assignmentResult.id,
            (string)assignmentResult.title,
            (string)(assignmentResult.description ?? ""),
            (DateTime)assignmentResult.deadline,
            (string)assignmentResult.assignment_type,
            (bool)assignmentResult.show_answers,
            assignmentResult.time_limit_minutes != null ? (int?)assignmentResult.time_limit_minutes : null,
            attachmentUrls,
            (decimal)assignmentResult.max_score,
            (bool)assignmentResult.is_published,
            (DateTime)assignmentResult.created_at,
            (DateTime)assignmentResult.updated_at,
            (string)assignmentResult.course_name,
            (string)assignmentResult.class_name,
            (int)assignmentResult.total_students,
            (int)assignmentResult.submissions_count,
            (int)assignmentResult.graded_submissions,
            (int)assignmentResult.pending_submissions,
            assignmentResult.average_grade != null ? (decimal?)assignmentResult.average_grade : null,
            recentSubmissionsList
        );

        return Result.Success(response);
    }

    private static List<string>? ParseAttachmentUrls(object? attachmentUrlsData)
    {
        if (attachmentUrlsData == null || attachmentUrlsData is DBNull)
            return null;

        try
        {
            switch (attachmentUrlsData)
            {
                case string[] stringArray:
                    return stringArray.ToList();
                case string jsonString when !string.IsNullOrEmpty(jsonString):
                    if (jsonString.StartsWith("["))
                    {
                        return JsonSerializer.Deserialize<List<string>>(jsonString);
                    }
                    return new List<string> { jsonString };
                default:
                    return null;
            }
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
