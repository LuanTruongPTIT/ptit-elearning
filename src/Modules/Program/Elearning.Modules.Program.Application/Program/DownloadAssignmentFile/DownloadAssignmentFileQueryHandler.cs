using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Program.Application.Program.DownloadAssignmentFile;

internal sealed class DownloadAssignmentFileQueryHandler : IQueryHandler<DownloadAssignmentFileQuery, DownloadAssignmentFileResponse>
{
  private readonly IDbConnectionFactory _dbConnectionFactory;
  private readonly HttpClient _httpClient;

  public DownloadAssignmentFileQueryHandler(IDbConnectionFactory dbConnectionFactory, HttpClient httpClient)
  {
    _dbConnectionFactory = dbConnectionFactory;
    _httpClient = httpClient;
  }

  public async Task<Result<DownloadAssignmentFileResponse>> Handle(DownloadAssignmentFileQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();

    // Validate student has access to this assignment
    const string validateAccessSql = @"
            SELECT COUNT(1)
            FROM programs.table_assignments a
            INNER JOIN programs.table_teaching_assign_courses tac ON a.teaching_assign_course_id = tac.id
            INNER JOIN programs.classes c ON c.id = tac.class_id
            INNER JOIN programs.table_student_programs sp ON sp.program_id = c.program_id
            WHERE a.id = @AssignmentId 
                AND sp.student_id = @StudentId 
                AND a.is_published = true";

    var hasAccess = await connection.QueryFirstOrDefaultAsync<int>(validateAccessSql, new
    {
      AssignmentId = request.AssignmentId,
      StudentId = request.StudentId
    });

    if (hasAccess == 0)
    {
      return Result.Failure<DownloadAssignmentFileResponse>(
          Error.Failure("Assignment.AccessDenied", "You don't have access to this assignment"));
    }

    // Validate file URL is in assignment's attachment_urls
    const string validateFileSql = @"
            SELECT attachment_urls
            FROM programs.table_assignments
            WHERE id = @AssignmentId";

    var attachmentUrls = await connection.QueryFirstOrDefaultAsync<string[]>(validateFileSql, new
    {
      AssignmentId = request.AssignmentId
    });

    if (attachmentUrls == null || !attachmentUrls.Contains(request.FileUrl))
    {
      return Result.Failure<DownloadAssignmentFileResponse>(
          Error.NotFound("File.NotFound", "File not found in assignment attachments"));
    }

    try
    {
      // Download file from URL
      var response = await _httpClient.GetAsync(request.FileUrl, cancellationToken);

      if (!response.IsSuccessStatusCode)
      {
        return Result.Failure<DownloadAssignmentFileResponse>(
            Error.Failure("File.DownloadFailed", "Failed to download file from server"));
      }

      var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
      var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

      // Extract filename from URL or Content-Disposition header
      var fileName = ExtractFileName(request.FileUrl, response);

      return Result.Success(new DownloadAssignmentFileResponse(
          fileContent,
          fileName,
          contentType
      ));
    }
    catch (Exception ex)
    {
      return Result.Failure<DownloadAssignmentFileResponse>(
          Error.Failure("File.DownloadError", $"Error downloading file: {ex.Message}"));
    }
  }

  private static string ExtractFileName(string fileUrl, HttpResponseMessage response)
  {
    // Try to get filename from Content-Disposition header
    if (response.Content.Headers.ContentDisposition?.FileName != null)
    {
      return response.Content.Headers.ContentDisposition.FileName.Trim('"');
    }

    // Extract from URL
    var uri = new Uri(fileUrl);
    var fileName = Path.GetFileName(uri.LocalPath);

    if (string.IsNullOrEmpty(fileName))
    {
      fileName = "attachment";
    }

    return fileName;
  }
}
