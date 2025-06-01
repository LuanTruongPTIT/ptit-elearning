using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using System.Text.Json;

namespace Elearning.Modules.Program.Application.Program.GetAssignmentDetails;

internal sealed class GetAssignmentDetailsQueryHandler : IQueryHandler<GetAssignmentDetailsQuery, GetAssignmentDetailsResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetAssignmentDetailsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<GetAssignmentDetailsResponse>> Handle(GetAssignmentDetailsQuery request, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

        const string sql = @"
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
                tac.course_class_name as course_name,
                u.full_name as teacher_name,
                CASE WHEN sub.id IS NOT NULL THEN true ELSE false END as has_submission,
                sub.id as submission_id,
                sub.submission_text,
                sub.file_attachments,
                sub.submitted_at,
                sub.points,
                sub.feedback
            FROM programs.table_assignments a
            INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
            INNER JOIN users.table_users u ON tac.teacher_id = u.id
            LEFT JOIN programs.table_assignment_submissions sub ON a.id = sub.assignment_id AND sub.student_id = @StudentId
            WHERE a.id = @AssignmentId
                AND a.is_published = true";

        var result = await connection.QueryFirstOrDefaultAsync(sql, new
        {
            AssignmentId = request.AssignmentId,
            StudentId = request.StudentId
        });

        if (result == null)
        {
            return Result.Failure<GetAssignmentDetailsResponse>(
                Error.NotFound("Assignment.NotFound", "Assignment not found or not accessible"));
        }

        AssignmentSubmissionInfo? submission = null;
        if (result.has_submission)
        {
            // Parse file_attachments - handle PostgreSQL text[] array
            List<string>? fileUrls = null;

            Console.WriteLine($"Debug: file_attachments raw value: '{result.file_attachments}' (Type: {result.file_attachments?.GetType()})");

            if (result.file_attachments != null)
            {
                try
                {
                    // Check if it's already a string array (PostgreSQL array type)
                    if (result.file_attachments is string[] stringArray)
                    {
                        fileUrls = stringArray.ToList();
                        Console.WriteLine($"Debug: Converted file_attachments string array to List<string>: {JsonSerializer.Serialize(fileUrls)}");
                    }
                    // Check if it's a List<string>
                    else if (result.file_attachments is List<string> stringList)
                    {
                        fileUrls = stringList;
                        Console.WriteLine($"Debug: file_attachments already a List<string>: {JsonSerializer.Serialize(fileUrls)}");
                    }
                    // Try to parse as JSON string (fallback)
                    else
                    {
                        string fileAttachmentsStr = result.file_attachments.ToString();
                        if (!string.IsNullOrEmpty(fileAttachmentsStr) && fileAttachmentsStr != "null")
                        {
                            // Handle case where it might be a JSON array
                            if (fileAttachmentsStr.StartsWith("[") && fileAttachmentsStr.EndsWith("]"))
                            {
                                fileUrls = JsonSerializer.Deserialize<List<string>>(fileAttachmentsStr);
                            }
                            else if (!fileAttachmentsStr.StartsWith("\""))
                            {
                                // If it's not a JSON array and not a quoted string, treat as single URL
                                fileUrls = new List<string> { fileAttachmentsStr };
                            }
                            else
                            {
                                // Try to deserialize as JSON string
                                var singleUrl = JsonSerializer.Deserialize<string>(fileAttachmentsStr);
                                if (!string.IsNullOrEmpty(singleUrl))
                                {
                                    fileUrls = new List<string> { singleUrl };
                                }
                            }

                            Console.WriteLine($"Debug: parsed file_attachments from string: {JsonSerializer.Serialize(fileUrls)}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Debug: Failed to parse file_attachments: {ex.Message}");
                    // If parsing fails, treat as null
                    fileUrls = null;
                }
            }
            else
            {
                Console.WriteLine("Debug: file_attachments is null");
            }

            submission = new AssignmentSubmissionInfo(
                result.submission_id,
                result.submission_text?.ToString() ?? "file",
                fileUrls,
                result.submitted_at,
                result.points,
                result.feedback?.ToString(),
                "submitted"
            );
        }

        // Parse attachment_urls - handle both JSON string and PostgreSQL array
        List<string>? attachmentUrls = null;

        Console.WriteLine($"Debug: attachment_urls raw value: '{result.attachment_urls}' (Type: {result.attachment_urls?.GetType()})");

        if (result.attachment_urls != null)
        {
            try
            {
                // Check if it's already a string array (PostgreSQL array type)
                if (result.attachment_urls is string[] stringArray)
                {
                    attachmentUrls = stringArray.ToList();
                    Console.WriteLine($"Debug: Converted string array to List<string>: {JsonSerializer.Serialize(attachmentUrls)}");
                }
                // Check if it's a List<string>
                else if (result.attachment_urls is List<string> stringList)
                {
                    attachmentUrls = stringList;
                    Console.WriteLine($"Debug: Already a List<string>: {JsonSerializer.Serialize(attachmentUrls)}");
                }
                // Try to parse as JSON string
                else
                {
                    string attachmentUrlsStr = result.attachment_urls.ToString();
                    if (!string.IsNullOrEmpty(attachmentUrlsStr) && attachmentUrlsStr != "null")
                    {
                        // Handle case where it might be a single string instead of JSON array
                        if (attachmentUrlsStr.StartsWith("[") && attachmentUrlsStr.EndsWith("]"))
                        {
                            attachmentUrls = JsonSerializer.Deserialize<List<string>>(attachmentUrlsStr);
                        }
                        else if (!attachmentUrlsStr.StartsWith("\""))
                        {
                            // If it's not a JSON array and not a quoted string, treat as single URL
                            attachmentUrls = new List<string> { attachmentUrlsStr };
                        }
                        else
                        {
                            // Try to deserialize as JSON string
                            var singleUrl = JsonSerializer.Deserialize<string>(attachmentUrlsStr);
                            if (!string.IsNullOrEmpty(singleUrl))
                            {
                                attachmentUrls = new List<string> { singleUrl };
                            }
                        }

                        Console.WriteLine($"Debug: parsed attachment_urls from string: {JsonSerializer.Serialize(attachmentUrls)}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Debug: Failed to parse attachment_urls: {ex.Message}");
                // If parsing fails, treat as null
                attachmentUrls = null;
            }
        }
        else
        {
            Console.WriteLine("Debug: attachment_urls is null");
        }

        var response = new GetAssignmentDetailsResponse(
            result.id,
            result.title,
            result.description ?? "",
            result.deadline,
            result.assignment_type,
            result.show_answers,
            result.time_limit_minutes,
            attachmentUrls,
            result.max_score,
            result.is_published,
            result.created_at,
            result.course_name,
            result.teacher_name,
            result.has_submission,
            submission
        );

        return Result.Success(response);
    }
}
