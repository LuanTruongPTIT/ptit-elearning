using System.Data.Common;
using Dapper;
using Elearning.Common.Application.Data;
using Elearning.Common.Application.Messaging;
using Elearning.Common.Domain;

namespace Elearning.Modules.Users.Application.Students.CreateStudent;

internal sealed class CreateStudentCommandHandler(IDbConnectionFactory dbConnectionFactory)
    : ICommandHandler<CreateStudentCommand, string>
{
  public async Task<Result<string>> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync();
    await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

    try
    {
      // Check if email already exists
      var emailExist = await CheckEmailExist(request.email, connection, transaction);
      if (emailExist)
      {
        return Result.Failure<string>(Error.Validation("Email", "Email is already in use"));
      }

      // Check if username already exists
      var usernameExist = await CheckUsernameExist(request.username, connection, transaction);
      if (usernameExist)
      {
        return Result.Failure<string>(Error.Validation("Username", "Username is already in use"));
      }

      // Hash password
      string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.password);

      // Create student user
      var studentId = await CreateStudentUser(request, passwordHash, connection, transaction);

      // Assign student to program if provided
      if (request.program_id.HasValue)
      {
        await AssignStudentToProgram(studentId, request.program_id.Value, connection, transaction);
      }

      // TODO: Send email if requested
      if (request.send_email)
      {
        // Implement email sending logic
      }

      await transaction.CommitAsync(cancellationToken);

      return Result.Success(studentId.ToString());
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
      await transaction.RollbackAsync(cancellationToken);
      return Result.Failure<string>(Error.Failure("CreateStudent", $"An error occurred: {ex.Message}"));
    }
  }

  private async Task<bool> CheckEmailExist(string email, DbConnection connection, DbTransaction transaction)
  {
    const string sql = "SELECT COUNT(1) FROM users.table_users WHERE email = @email";
    var count = await connection.ExecuteScalarAsync<int>(sql, new { email }, transaction);
    return count > 0;
  }

  private async Task<bool> CheckUsernameExist(string username, DbConnection connection, DbTransaction transaction)
  {
    const string sql = "SELECT COUNT(1) FROM users.table_users WHERE username = @username";
    var count = await connection.ExecuteScalarAsync<int>(sql, new { username }, transaction);
    return count > 0;
  }

  private async Task<Guid> CreateStudentUser(CreateStudentCommand request, string passwordHash, DbConnection connection, DbTransaction transaction)
  {
    const string sqlUser = @"
            INSERT INTO users.table_users(
                id, username, email, password_hash, full_name, 
                phone_number, address, date_of_birth, gender, account_status, 
                created_at, updated_at
            )
            VALUES (
                @id, @username, @email, @passwordHash, @fullName, 
                @phoneNumber, @address, @dateOfBirth, @gender, @accountStatus, 
                @createdAt, @updatedAt
            )
            RETURNING id
        ";

    var studentId = Guid.NewGuid();
    var now = DateTime.UtcNow;

    await connection.ExecuteAsync(
        sqlUser,
        new
        {
          id = studentId,
          username = request.username,
          email = request.email,
          passwordHash,
          fullName = request.full_name,
          phoneNumber = request.phone_number,
          address = request.address,
          dateOfBirth = request.date_of_birth,
          gender = request.gender == "Male" ? 1 : 2, // Default to Male if not provided
          accountStatus = 1, // Active
          createdAt = now,
          updatedAt = now
        },
        transaction
    );

    // Assign Student role
    const string sqlRole = @"
            INSERT INTO users.table_user_roles(user_id, role_name)
            VALUES (@userId, @roleName)
        ";

    await connection.ExecuteAsync(
        sqlRole,
        new
        {
          userId = studentId,
          roleName = "Student"
        },
        transaction
    );

    return studentId;
  }

  private async Task AssignStudentToProgram(Guid studentId, Guid programId, DbConnection connection, DbTransaction transaction)
  {
    const string sql = @"
            INSERT INTO programs.table_student_programs(id, student_id, program_id, created_at)
            VALUES (@id, @studentId, @programId, @createdAt)
        ";
    Console.WriteLine(sql);
    await connection.ExecuteAsync(
        sql,
        new
        {
          id = Guid.NewGuid(),
          studentId,
          programId,
          createdAt = DateTime.UtcNow
        },
        transaction
    );
  }
}
