using Microsoft.Extensions.DependencyInjection;

namespace Common.SharedKernel.Application.Rpc;

public static class RpcServiceCollectionExtensions
{
    public static IServiceCollection AddInMemoryRpc(this IServiceCollection services)
    {
        services.AddSingleton<IRpcClient, InMemoryRpcClient>();
        return services;
    }
}
