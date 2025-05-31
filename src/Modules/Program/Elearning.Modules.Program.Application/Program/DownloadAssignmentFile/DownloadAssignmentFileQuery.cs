using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.DownloadAssignmentFile;

public sealed record DownloadAssignmentFileQuery(
    Guid AssignmentId,
    string FileUrl,
    Guid StudentId
) : IQuery<DownloadAssignmentFileResponse>;

public sealed record DownloadAssignmentFileResponse(
    byte[] FileContent,
    string FileName,
    string ContentType
);
