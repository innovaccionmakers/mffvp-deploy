using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Configuration.Strategies;
using DotNetCore.CAP;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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

        services.AddSingleton<IEventBus, EventBus.EventBus>();

        return services;
    }
}