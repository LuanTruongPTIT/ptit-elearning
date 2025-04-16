namespace Elearning.Api.Middleware;

internal static class MiddlewareExtensions
{
  internal static IApplicationBuilder UseLogContext(this IApplicationBuilder app)
  {
    app.UseMiddleware<LoggContextTraceLoggingMiddleware>();
    return app;
  }
}