using Common.SharedKernel.Domain;
using MediatR;

namespace Common.SharedKernel.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;