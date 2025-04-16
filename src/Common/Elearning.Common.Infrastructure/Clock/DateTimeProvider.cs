using Elearning.Common.Application.Clock;

namespace Common.Elearning.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
  public DateTime UtcNow => DateTime.UtcNow;
}