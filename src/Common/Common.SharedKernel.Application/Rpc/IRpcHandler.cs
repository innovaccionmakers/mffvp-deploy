namespace Common.SharedKernel.Application.Rpc;

public interface IRpcHandler<in TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}
