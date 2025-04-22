public sealed record GetCoursesByProgramAndTeacherResponse
{
  public Guid course_id { get; set; }
  public string course_name { get; set; }
  public string course_code { get; set; }
  public Guid program_id { get; set; }
  public string program_name { get; set; }
}