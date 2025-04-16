using System.Diagnostics;
using Serilog.Context;

namespace Elearning.Api.Middleware;

internal sealed class LoggContextTraceLoggingMiddleware(RequestDelegate next)
{
  public Task Invoke(HttpContext context)
  {
    string traceId = Activity.Current?.TraceId.ToString();
    using (LogContext.PushProperty("TraceID", traceId))
    {
      return next.Invoke(context);
    }
  }
}