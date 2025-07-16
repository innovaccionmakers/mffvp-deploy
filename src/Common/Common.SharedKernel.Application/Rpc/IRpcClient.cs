namespace Common.SharedKernel.Application.Rpc;

public interface IRpcClient
{
    Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request, CancellationToken ct);
}
