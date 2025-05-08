using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.Configuration.Strategies;
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
        string mongoDbConnectionString,
        string databaseConnectionStringSQL
        )
    {
        services.AddSingleton<IMongoClient>(new MongoClient(mongoDbConnectionString));

        // services.AddCap(x =>
        // {
        //     x.UseInMemoryStorage();
        //     x.UseInMemoryMessageQueue();
        //     x.UseMongoDB(mongoDbConnectionString);
        //     x.FailedRetryInterval = 5;
        //     x.FailedRetryCount = 10;

        // });

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

        return services;
    }
}
