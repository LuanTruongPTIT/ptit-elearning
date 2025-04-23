using System;

namespace Elearning.Modules.Program.Application.Program.GetPrograms;

public class GetProgramsResponse
{
  public Guid id { get; set; }
  public string name { get; set; }
  public string code { get; set; }
  public Guid department_id { get; set; }
  public string department_name { get; set; }
  public DateTime created_at { get; set; }
  public DateTime updated_at { get; set; }

  // Parameterless constructor for Dapper
  public GetProgramsResponse()
  {
  }

  // Constructor with parameters
  public GetProgramsResponse(
      Guid id,
      string name,
      string code,
      Guid department_id,
      string department_name,
      DateTime created_at,
      DateTime updated_at)
  {
    this.id = id;
    this.name = name;
    this.code = code;
    this.department_id = department_id;
    this.department_name = department_name;
    this.created_at = created_at;
    this.updated_at = updated_at;
  }
}
