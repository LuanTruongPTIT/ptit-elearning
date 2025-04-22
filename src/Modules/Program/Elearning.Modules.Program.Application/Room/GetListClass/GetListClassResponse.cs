namespace Elearning.Modules.Program.Application.Room.GetListClass;

public sealed class GetListClassResponse
{
    public Guid id { get; set; }
    public string class_name { get; set; }
    public Guid department_id { get; set; }
    public string department_name { get; set; }
    public Guid program_id { get; set; }
    public string program_name { get; set; }
    public string academic_period { get; set; }
    public string status { get; set; }
    public DateTime created_at { get; set; }
}