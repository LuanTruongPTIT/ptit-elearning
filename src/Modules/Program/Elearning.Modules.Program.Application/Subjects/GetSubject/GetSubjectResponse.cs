using System;

namespace Elearning.Modules.Program.Application.Subjects.GetSubject;

public class GetSubjectResponse
{
    public Guid id { get; set; }
    public string code { get; set; }
    public string name { get; set; }
    public Guid department_id { get; set; }
    public string department_name { get; set; }
    public int credits { get; set; }
    public string description { get; set; }
    public bool is_active { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    
    // Parameterless constructor for Dapper
    public GetSubjectResponse()
    {
    }
}
