using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetCourseDetails;

public sealed record GetCourseDetailsQuery(
    Guid course_id,
    Guid student_id
) : IQuery<GetCourseDetailsResponse>;

public class GetCourseDetailsResponse
{
  public Guid Id { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
  public string ThumbnailUrl { get; set; }
  public int Progress { get; set; }
  public int TotalLectures { get; set; }
  public int CompletedLectures { get; set; }
  public string TeacherName { get; set; }
  public string TeacherAvatar { get; set; }
  public string Syllabus { get; set; }
  public List<CourseSectionDto> Sections { get; set; } = new();

  // Parameterless constructor for Dapper
  public GetCourseDetailsResponse() { }
}

public class CourseSectionDto
{
  public Guid Id { get; set; }
  public string Title { get; set; }
  public List<LectureDto> Lectures { get; set; } = new();
}

public class LectureDto
{
  public Guid Id { get; set; }
  public string Title { get; set; }
  public string Description { get; set; }
  public int Duration { get; set; } // in minutes
  public string Type { get; set; } // video, document, quiz
  public bool IsCompleted { get; set; }
  public string ContentUrl { get; set; }
}