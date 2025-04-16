using Elearning.Common.Domain;
using MediatR;

namespace Elearning.Common.Application.Messaging;


public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>> where TQuery : IQuery<TResponse>;