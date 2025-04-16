using System.Data;
using Dapper;

internal sealed class GenericArrayHandler<T> : SqlMapper.TypeHandler<T[]>
{
  public override T[]? Parse(object value)
  {
    return value as T[];
  }

  public override void SetValue(IDbDataParameter parameter, T[]? value)
  {
    parameter.Value = value;
  }
}