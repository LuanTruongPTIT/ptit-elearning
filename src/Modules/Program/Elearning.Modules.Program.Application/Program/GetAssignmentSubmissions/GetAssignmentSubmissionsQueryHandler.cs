using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Text.Json;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentSubmissions;

// Temporary DTO for database mapping
internal class SubmissionRawDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string? StudentAvatar { get; set; }
    public string SubmissionType { get; set; } = string.Empty;
    public string FileUrlsJson { get; set; } = "[]";
    public string? TextContent { get; set; }
    public DateTime SubmittedAt { get; set; }
    public decimal? Grade { get; set; }
    public string? Feedback { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsLate { get; set; }
    public TimeSpan? LateDuration { get; set; }
}

internal sealed class GetAssignmentSubmissionsQueryHandler(
    IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetAssignmentSubmissionsQuery, GetAssignmentSubmissionsResponse>
{
    public async Task<Result<GetAssignmentSubmissionsResponse>> Handle(GetAssignmentSubmissionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

            // Verify teacher has access to this assignment
            var teacherAccessQuery = @"
                SELECT COUNT(*)
                FROM programs.table_assignments a
                INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
                WHERE a.id = @AssignmentId AND tac.teacher_id = @TeacherId";

            var hasAccess = await connection.QuerySingleAsync<int>(teacherAccessQuery, new { request.AssignmentId, request.TeacherId });

            if (hasAccess == 0)
            {
                return Result.Failure<GetAssignmentSubmissionsResponse>(
                    new Error("GetAssignmentSubmissions.Unauthorized", "You don't have access to this assignment", ErrorType.Authorization));
            }

            // Get assignment info
            var assignmentQuery = @"
                SELECT 
                    a.id as Id,
                    a.title as Title,
                    a.description as Description,
                    a.deadline as Deadline,
                    a.max_score as MaxScore,
                    COUNT(asub.id) as TotalSubmissions,
                    COUNT(CASE WHEN asub.grade IS NOT NULL THEN 1 END) as GradedSubmissions,
                    COUNT(CASE WHEN asub.grade IS NULL THEN 1 END) as PendingSubmissions
                FROM programs.table_assignments a
                LEFT JOIN programs.table_assignment_submissions asub ON a.id = asub.assignment_id
                WHERE a.id = @AssignmentId
                GROUP BY a.id, a.title, a.description, a.deadline, a.max_score";

            var assignment = await connection.QuerySingleAsync<AssignmentInfoDto>(assignmentQuery, new { request.AssignmentId });

            // Build submissions query with filters
            var whereClause = "WHERE asub.assignment_id = @AssignmentId";
            var parameters = new Dictionary<string, object>
            {
                ["AssignmentId"] = request.AssignmentId,
                ["Offset"] = (request.Page - 1) * request.PageSize,
                ["PageSize"] = request.PageSize
            };

            if (!string.IsNullOrEmpty(request.Status))
            {
                whereClause += " AND asub.status = @Status";
                parameters["Status"] = request.Status;
            }

            // Get submissions with pagination
            var submissionsQuery = $@"
                SELECT 
                    asub.id as Id,
                    asub.student_id as StudentId,
                    u.full_name as StudentName,
                    u.email as StudentEmail,
                    u.avatar_url as StudentAvatar,
                    COALESCE(asub.submission_type, 'file') as SubmissionType,
                    COALESCE(array_to_json(asub.file_attachments)::text, '[]') as FileUrlsJson,
                    asub.text_content as TextContent,
                    asub.submitted_at as SubmittedAt,
                    asub.grade as Grade,
                    asub.feedback as Feedback,
                    COALESCE(asub.status, 'submitted') as Status,
                    CASE WHEN asub.submitted_at > a.deadline THEN true ELSE false END as IsLate,
                    CASE WHEN asub.submitted_at > a.deadline 
                         THEN asub.submitted_at - a.deadline 
                         ELSE NULL END as LateDuration
                FROM programs.table_assignment_submissions asub
                INNER JOIN users.table_users u ON asub.student_id = u.id
                INNER JOIN programs.table_assignments a ON asub.assignment_id = a.id
                {whereClause}
                ORDER BY 
                    CASE WHEN asub.grade IS NULL THEN 0 ELSE 1 END,
                    asub.submitted_at DESC
                LIMIT @PageSize OFFSET @Offset";

            Console.WriteLine(submissionsQuery);
            var rawSubmissions = await connection.QueryAsync<SubmissionRawDto>(submissionsQuery, parameters);

            // Get total count for pagination
            var countQuery = $@"
                SELECT COUNT(*)
                FROM programs.table_assignment_submissions asub
                {whereClause}";

            var totalCount = await connection.QuerySingleAsync<int>(countQuery, parameters);

            // Convert raw submissions to final DTOs
            var submissions = rawSubmissions.Select(raw => new AssignmentSubmissionDto
            {
                Id = raw.Id,
                StudentId = raw.StudentId,
                StudentName = raw.StudentName,
                StudentEmail = raw.StudentEmail,
                StudentAvatar = raw.StudentAvatar,
                SubmissionType = raw.SubmissionType,
                FileUrls = ParseFileUrls(raw.FileUrlsJson),
                TextContent = raw.TextContent,
                SubmittedAt = raw.SubmittedAt,
                Grade = raw.Grade,
                Feedback = raw.Feedback,
                Status = raw.Status,
                IsLate = raw.IsLate,
                LateDuration = raw.LateDuration
            }).ToList();

            var response = new GetAssignmentSubmissionsResponse
            {
                Assignment = assignment,
                Submissions = submissions,
                Pagination = new PaginationDto
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                    HasNextPage = request.Page * request.PageSize < totalCount,
                    HasPreviousPage = request.Page > 1
                }
            };

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return Result.Failure<GetAssignmentSubmissionsResponse>(
                      new Error("GetAssignmentSubmissions.DatabaseError", $"Database error: {ex.Message}", ErrorType.Failure));
        }
    }

    private static List<string> ParseFileUrls(string fileUrlsJson)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrlsJson) || fileUrlsJson == "null")
                return new List<string>();

            return JsonSerializer.Deserialize<List<string>>(fileUrlsJson) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
