namespace Elearning.Modules.Program.Application.Program.GetRecentCourses
{
    public class GetRecentCoursesResponse
    {
        public List<EnrolledCourseDto> Courses { get; set; }
    }

    public class EnrolledCourseDto
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Thumbnail { get; set; }
        public string Instructor { get; set; }
        public int Progress { get; set; }
        public int TotalLectures { get; set; }
        public int CompletedLectures { get; set; }
        public string LastAccessed { get; set; }
        public string Category { get; set; }
        public string Status { get; set; }
    }
}
