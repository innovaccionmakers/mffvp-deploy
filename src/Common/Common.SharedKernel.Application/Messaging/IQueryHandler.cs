using Common.SharedKernel.Domain;
using MediatR;

namespace Common.SharedKernel.Application.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;