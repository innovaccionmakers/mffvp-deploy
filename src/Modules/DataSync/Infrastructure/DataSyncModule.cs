using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rpc;
using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Services;
using DataSync.Application.TrustSync.Services.Interfaces;
using DataSync.Infrastructure.ConnectionFactory;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;
using DataSync.Infrastructure.TrustSync;
using DataSync.IntegrationEvents.TrustSync;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataSync.Infrastructure;

public class DataSyncModule : IModuleConfiguration
{
    public string ModuleName => "DataSync";
    public string RoutePrefix => "api/datasync";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Conexiones por dominio
        services.AddScoped<ITrustConnectionFactory, TrustConnectionFactory>();
        services.AddScoped<IClosingConnectionFactory, ClosingConnectionFactory>();

        // Adaptadores externos
        services.AddScoped<ITrustReader, TrustReader>();
        services.AddScoped<IClosingTrustYieldMerger, ClosingTrustYieldMerger>();
        services.AddScoped<ITrustUnitsUpdater, TrustUnitsUpdater>();

        // Servicios 
        services.AddScoped<ITrustSyncClosingService, TrustSyncClosingService>();
        services.AddScoped<ITrustSyncPostService, TrustSyncPostService>();

        // RPC (CAP)
        services.AddScoped<IRpcHandler<TrustSyncRequest, TrustSyncResponse>, TrustSyncConsumer>();
        services.AddScoped<IRpcHandler<TrustSyncPostRequest, TrustSyncPostResponse>, TrustSyncPostConsumer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        
    }
}
