using Common.SharedKernel.Application.Abstractions;
using MFFVP.BFF.GraphQL;
using MFFVP.BFF.Services;
using MFFVP.BFF.Services.Reports;
using MFFVP.BFF.Services.Reports.DepositsReport;
using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;
using MFFVP.BFF.Services.Reports.Interfaces;
using MFFVP.BFF.Services.Reports.Strategies;

namespace MFFVP.BFF;

public class ModuleConfiguration : IModuleConfiguration
{
    public string ModuleName => "BFF";
    public string RoutePrefix => "graphql";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

        services.AddScoped<ExperienceOrchestrator>();
        services.AddScoped<ReportOrchestrator>();
        services.AddScoped<PaymentMethodProcessor>();
        services.AddScoped<IExcelReportService, ExcelReportService>();
        services.AddScoped<IReportStrategy, DepositsReportStrategy>();
        services.AddScoped<IDepositsReportDataProvider, DepositsReportDataProvider>();

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