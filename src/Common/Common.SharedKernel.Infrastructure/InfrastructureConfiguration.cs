using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Configuration.Strategies;
using Common.SharedKernel.Infrastructure.EventBus;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Savorboard.CAP.InMemoryMessageQueue;

namespace Common.SharedKernel.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string serviceName,
        string databaseConnectionString,
        string capDbConnectionString,
        string databaseConnectionStringSQL
    )
    {
        services.AddCap(x =>
        {
            x.UseInMemoryStorage();
            x.UseInMemoryMessageQueue();
            x.UsePostgreSql(capDbConnectionString);
            x.FailedRetryInterval = 5;
            x.FailedRetryCount = 10;
            x.UseDashboard();
        });

        services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddNpgsql();

                tracing.AddOtlpExporter();
            });

        services.AddScoped<IDatabaseConnectionStrategy, SqlServerConnectionStrategy>();
        services.AddScoped<IDatabaseConnectionStrategy, YugaByteConnectionStrategy>();
        services.AddScoped<DatabaseConnectionContext>();
        services.AddSingleton<ICapRpcClient, CapRpcClient>();
        services.AddSingleton<CapCallbackSubscriber>();

        return services;
    }
}