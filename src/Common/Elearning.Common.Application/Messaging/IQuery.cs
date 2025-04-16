
using Elearning.Common.Domain;
using MediatR;

namespace Elearning.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;