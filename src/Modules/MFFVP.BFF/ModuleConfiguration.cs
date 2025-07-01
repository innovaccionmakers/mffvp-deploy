using Common.SharedKernel.Application.Abstractions;
using MFFVP.BFF.GraphQL;
using MFFVP.BFF.Services;

namespace MFFVP.BFF;

public class ModuleConfiguration : IModuleConfiguration
{
    public string ModuleName => "BFF";
    public string RoutePrefix => "graphql";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<ExperienceOrchestrator>();

        // Add GraphQL Schema Stitching
        services.AddSchemaStitching(configuration);

        // Configuración específica por ambiente
        if (configuration.GetValue("Development:IsEnabled", true))
        {
            services.AddDevelopmentConfiguration(configuration);
        }
        else
        {
            services.AddProductionConfiguration(configuration);
        }
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapGraphQL($"/{RoutePrefix}", "BFFGateway");
        });
    }
}