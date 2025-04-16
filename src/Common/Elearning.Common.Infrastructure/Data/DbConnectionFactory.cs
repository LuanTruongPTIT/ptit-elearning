using System.Data.Common;
using Elearning.Common.Application.Data;
using Npgsql;

namespace Elearning.Common.Infrastructure.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
  public async ValueTask<DbConnection> OpenConnectionAsync()
  {
    return await dataSource.OpenConnectionAsync();
  }
}