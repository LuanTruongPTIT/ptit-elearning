using System;

namespace Elearning.Modules.Users.Application.Students.GetStudent;

public class GetStudentResponse
{
  public Guid id { get; set; }
  public string username { get; set; }
  public string email { get; set; }
  public string full_name { get; set; }
  public string phone_number { get; set; }
  public string address { get; set; }
  public string avatar_url { get; set; }
  public DateTime? date_of_birth { get; set; }
  public int gender { get; set; }
  public int account_status { get; set; }
  public DateTime created_at { get; set; }
  public Guid? program_id { get; set; }
  public string program_name { get; set; }

  // Parameterless constructor for Dapper
  public GetStudentResponse()
  {
  }
}
