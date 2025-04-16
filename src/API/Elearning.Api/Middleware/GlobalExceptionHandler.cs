using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Elearning.Api.Middleware;

internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
  {
    logger.LogError(exception, "Unhandled exception occurred");

    var problemDetails = new ProblemDetails
    {
      Status = StatusCodes.Status500InternalServerError,
      Type = "https://httpstatuses.com/500",
      Title = "Server failuresss",
      Detail = exception.Message,
      Instance = httpContext.Request.Path,
    };

    problemDetails.Extensions["traceId"] = Activity.Current?.TraceId.ToString();
    problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
    problemDetails.Extensions["stackTrace"] = exception.StackTrace;
    if (exception.InnerException is not null)
    {
      logger.LogError(exception.InnerException.StackTrace, "Unhandled inner exception occurred");
      problemDetails.Extensions["innerException"] = new
      {
        message = exception.InnerException.Message,
        type = exception.InnerException.GetType().Name,
        stackTrace = exception.InnerException.StackTrace
      };
    }

    httpContext.Response.StatusCode = problemDetails.Status.Value;
    await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
    return true;
  }
}
