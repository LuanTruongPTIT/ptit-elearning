namespace Elearning.Modules.Program.Application.Program.GetTeachingAssignCourses;

public sealed class GetTeachingAssignCoursesResponse
{
  public Guid id { get; set; }
  public string course_class_name { get; set; }
  public string description { get; set; }
  public Guid class_id { get; set; }
  public string class_name { get; set; }
  public Guid course_id { get; set; }
  public string course_name { get; set; }
  public string course_code { get; set; }
  public DateTime start_date { get; set; }
  public DateTime end_date { get; set; }
  public string thumbnail_url { get; set; }
  public string status { get; set; }
  public DateTime created_at { get; set; }
}