using System.Data.Common;
using Elearning.Common.Application.Data;
using Npgsql;

namespace Elearning.Common.Infrastructure.Database;

public sealed class PostgresConnectionFactory : IDbConnectionFactory
{
  private readonly string _connectionString;

  public PostgresConnectionFactory(string connectionString)
  {
    _connectionString = connectionString;
  }

  public async ValueTask<DbConnection> OpenConnectionAsync()
  {
    var connection = new NpgsqlConnection(_connectionString);
    await connection.OpenAsync();
    return connection;
  }
}