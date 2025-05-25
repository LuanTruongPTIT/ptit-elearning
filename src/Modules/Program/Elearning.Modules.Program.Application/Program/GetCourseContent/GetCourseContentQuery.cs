using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetCourseContent;

public sealed record GetCourseContentQuery(
    Guid CourseId,
    Guid StudentId
) : IQuery<CourseDetails>;

public class CourseDetails
{
    public Guid course_id { get; set; }
    public string course_name { get; set; }
    public string description { get; set; }
    public string thumbnail_url { get; set; }
    public Instructor instructor { get; set; }
    public int progress_percent { get; set; }
    public int total_lectures { get; set; }
    public int completed_lectures { get; set; }
    public string last_accessed { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
    public string status { get; set; }
    public List<Lecture> lectures { get; set; } = new();
    public List<Announcement> announcements { get; set; } = new();
    public List<Resource> resources { get; set; } = new();

    // Parameterless constructor for Dapper
    public CourseDetails()
    {
        instructor = new Instructor();
    }
}

public class Instructor
{
    public string name { get; set; }
    public string avatar { get; set; }
    public string email { get; set; }
}

public class Lecture
{
    public Guid id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string content_type { get; set; } // video, document, quiz, assignment, exam
    public string content_url { get; set; }
    public string youtube_video_id { get; set; }
    public bool is_completed { get; set; }
    public string created_at { get; set; }
    public string updated_at { get; set; }
}

public class Announcement
{
    public Guid id { get; set; }
    public string title { get; set; }
    public string content { get; set; }
    public string created_at { get; set; }
    public string author { get; set; }
}

public class Resource
{
    public Guid id { get; set; }
    public string title { get; set; }
    public string type { get; set; } // Assignment, Resource, Exam
    public string url { get; set; }
    public string created_at { get; set; }
}
