using Microsoft.Extensions.DependencyInjection;

namespace Common.SharedKernel.Application.Rpc;

internal sealed class InMemoryRpcClient(IServiceScopeFactory scopeFactory) : IRpcClient
{
    public async Task<TResponse> CallAsync<TRequest, TResponse>(TRequest request, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRpcHandler<TRequest, TResponse>>();
        return await handler.HandleAsync(request!, ct);
    }
}