using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;
using Elearning.Modules.Users.Application.Users.CreateTeacher;
namespace Elearning.Modules.Users.Application.Users.CreateTeacher;

public class CreateTeacherCommandHandler(IDbConnectionFactory dbConnectionFactory) : ICommandHandler<CreateTeacherCommand, string>
{
  public async Task<Result<string>> Handle(CreateTeacherCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync();
    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    try
    {
      var emailExist = await CheckEmailExist(request.email, connection, transaction);

      if (emailExist)
      {
        return Result.Failure<string>(Error.Validation("Email", "Email is already exist!"));
      }

      request.password = BCrypt.Net.BCrypt.HashPassword(request.password);

      var teacher_id = await CreateUserWithRoleTeacher(request, connection, transaction);
      await AssignTeachingSubject(request, teacher_id, connection, transaction);

      await transaction.CommitAsync(cancellationToken);

      return Result.Success(teacher_id.ToString());
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      await transaction.RollbackAsync(cancellationToken);
      return Result.Failure<string>(Error.Failure("CreateTeacher", $"An error occurred: {ex.Message}"));
    }
  }

  private async Task<bool> CheckEmailExist(string email, DbConnection connection, DbTransaction transaction)
  {
    const string sql =
        """
        SELECT EXISTS(
            SELECT 1
            FROM users.table_users
            WHERE email = @Email
        )
        """;

    return await connection.ExecuteScalarAsync<bool>(sql, new { Email = email }, transaction);
  }

  private async Task<Guid> CreateUserWithRoleTeacher(CreateTeacherCommand request, DbConnection connection, DbTransaction transaction)
  {
    const string sqlUser =
        """
        INSERT INTO users.table_users(id,email, username, password_hash, 
        full_name, phone_number, address, account_status, date_of_birth, gender)
        VALUES (@id, @email, @username, @password, @fullName, @phone, @address, @status, @birthday, @gender)
        RETURNING id
        """;

    var teacher_id = Guid.NewGuid();

    var userId = await connection.ExecuteScalarAsync<Guid>(
        sqlUser,
        new
        {
          id = teacher_id,
          request.email,
          request.username,
          request.password,
          request.fullName,
          request.phone,
          request.address,
          status = int.Parse(request.status),
          request.birthday,
          gender = int.Parse(request.gender)
        },
        transaction
    );

    const string sqlRole =
        """
        INSERT INTO users.table_user_roles(user_id, role_name)
        VALUES (@userId, 'Teacher')
        """;

    await connection.ExecuteAsync(sqlRole, new { userId = teacher_id }, transaction);

    return teacher_id;
  }

  private async Task AssignTeachingSubject(CreateTeacherCommand request, Guid teacherId, DbConnection connection, DbTransaction transaction)
  {
    const string sql =
        """
        INSERT INTO programs.table_teaching_assignments(id, teacher_id, department_id, employed_date, subjects)
        VALUES (@id, @teacher_id, @department_id, @employment_date, @subjects)
        """;
    Console.WriteLine(sql);
    await connection.ExecuteAsync(
        sql,
        new
        {
          id = Guid.NewGuid(),
          teacher_id = teacherId,
          department_id = request.department,
          employment_date = request.employmentDate,
          subjects = request.subjects
        },
        transaction
    );
  }
}
