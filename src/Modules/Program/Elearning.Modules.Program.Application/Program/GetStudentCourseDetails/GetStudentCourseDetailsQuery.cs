using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetStudentCourseDetails;

public sealed record GetStudentCourseDetailsQuery(
    Guid CourseId,
    Guid StudentId
) : IQuery<StudentCourseDetailsResponse>;

public class StudentCourseDetailsResponse
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; }
    public string Description { get; set; }
    public string ThumbnailUrl { get; set; }
    public double ProgressPercent { get; set; }
    public int TotalLectures { get; set; }
    public int CompletedLectures { get; set; }
    public DateTime? LastAccessed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Status { get; set; }

    // Instructor information
    public InstructorDto Instructor { get; set; }

    // Lectures list (without sections)
    public List<LectureDto> Lectures { get; set; } = new();

    // Announcements (mock data for now)
    public List<AnnouncementDto> Announcements { get; set; } = new();

    // Resources
    public List<ResourceDto> Resources { get; set; } = new();

    // Parameterless constructor for Dapper
    public StudentCourseDetailsResponse()
    {
        Instructor = new InstructorDto();
    }
}

public class InstructorDto
{
    public string TeacherName { get; set; }
    public string Avatar { get; set; }
}

public class LectureDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string ContentType { get; set; } // video, document, quiz, etc.
    public string ContentUrl { get; set; }
    public bool IsCompleted { get; set; }
}

public class AnnouncementDto
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; }
}

public class ResourceDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ContentType { get; set; }
    public string ContentUrl { get; set; }
}

public class AttachmentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string ContentType { get; set; }
    public string ContentUrl { get; set; }
}
