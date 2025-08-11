using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rpc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DataSync.Application.TrustSync;
using DataSync.Infrastructure.External.Trusts;
using DataSync.Infrastructure.External.Closing;
using DataSync.IntegrationEvents.TrustSync;

namespace DataSync.Infrastructure;

public class DataSyncModule : IModuleConfiguration
{
    public string ModuleName => "DataSync";
    public string RoutePrefix => "api/datasync";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITrustDataService, TrustDataService>();
        services.AddScoped<IYieldSyncService, YieldSyncService>();
        
        services.AddScoped<IRpcHandler<TrustSyncRequest, TrustSyncResponse>, TrustSyncConsumer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
    }
}
