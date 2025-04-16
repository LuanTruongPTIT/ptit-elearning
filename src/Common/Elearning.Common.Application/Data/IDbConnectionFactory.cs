using System.Data.Common;

namespace Elearning.Common.Application.Data;


public interface IDbConnectionFactory
{
  ValueTask<DbConnection> OpenConnectionAsync();
}