using Elearning.Common.Application.Messaging;

namespace Elearning.Modules.Program.Application.Program.GetTeachingAssignCourses;

public sealed class GetTeachingAssignCoursesQuery : IQuery<List<GetTeachingAssignCoursesResponse>>
{
    public string teacher_id { get; set; }
    public GetTeachingAssignCoursesQuery(string teacher_id)
    {
        this.teacher_id = teacher_id;
    }
}