using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Users.Application.Students.CreateStudent;

public sealed class CreateStudentCommand : ICommand<string>
{
    public string username { get; set; }
    public string email { get; set; }
    public string full_name { get; set; }
    public string? phone_number { get; set; }
    public string? address { get; set; }
    public DateTime? date_of_birth { get; set; }
    public string? gender { get; set; }
    public Guid? program_id { get; set; }
    public string password { get; set; }
    public bool send_email { get; set; } = true;
}
