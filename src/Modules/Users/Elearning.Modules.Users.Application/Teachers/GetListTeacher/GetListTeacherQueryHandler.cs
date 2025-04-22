using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Users.Application.Teachers.GetListTeacher;

internal sealed class GetListTeacherQueryHandler(IDbConnectionFactory dbConnectionFactory) : IQueryHandler<GetListTeacherQuery, List<GetListTeacherResponse>>
{

  public async Task<Result<List<GetListTeacherResponse>>> Handle(GetListTeacherQuery request, CancellationToken cancellationToken)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();
    int page = request.page ?? 1;
    int page_size = request.page_size ?? 10;
    var sql = @"
    SELECT
          u.username,
          u.email,
          u.full_name,
          u.phone_number,
          u.address,
          u.avatar_url,
          r.name AS role_name,
          u.date_of_birth,
          u.gender,
          u.account_status,
          u.id AS user_id,
          td.id AS department_id,
          td.name AS department_name,
          td.code AS deparment_Code,
          ta.subjects AS course_id,
          tc.id AS course_id,
          tc.name AS course_name,
          tc.code AS course_code
          
          FROM
              users.table_users u
              INNER JOIN users.table_user_roles ur ON u.id = ur.user_id
              INNER JOIN users.table_roles r ON r.name = ur.role_name
              INNER JOIN programs.table_teaching_assignments ta ON ta.teacher_id = u.id
              INNER JOIN programs.table_departments td ON td.id = ta.department_id
              JOIN LATERAL unnest(ta.subjects) AS sid ON TRUE
              JOIN programs.table_courses tc ON tc.id = sid
              LIMIT @pageSize OFFSET @offset
              ";
    Console.WriteLine(sql);

    var teacherDictionary = new Dictionary<Guid, GetListTeacherResponse>();
    var result = await connection.QueryAsync<GetListTeacherResponse, TeacherInformationTeaching, TeacherInformationSubjectAssigned, GetListTeacherResponse>(sql, (teacher, teaching, subject) =>
    {
      Console.WriteLine(@" teacher: " + teacher.role_name);
      if (!teacherDictionary.TryGetValue(teacher.user_id, out var existingTeacher))
      {
        teaching.courses = new List<TeacherInformationSubjectAssigned>();
        teacher.information_teaching = teaching;
        teacherDictionary.Add(teacher.user_id, teacher);
        existingTeacher = teacher;
      }

      existingTeacher.information_teaching.courses.Add(subject);
      return existingTeacher;
    },
     splitOn: "department_id, course_id",
     param: new { pageSize = page_size, offset = (page - 1) * page_size }
    );
    return teacherDictionary.Values.ToList();
  }
}