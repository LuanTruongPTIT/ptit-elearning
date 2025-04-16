using Elearning.Common.Domain;

namespace Elearning.Common.Application.Exceptions;



public sealed class ElearningException : Exception
{
  public ElearningException(string requestName, Error? error = default, Exception? innerException = default) : base("Application exception", innerException)
  {
    RequestName = requestName;
    Error = error;
  }

  public string RequestName { get; }
  public Error? Error { get; }
}