using Common.SharedKernel.Application.Abstractions;
using MFFVP.BFF.GraphQL;
using MFFVP.BFF.Services;
using MFFVP.BFF.Services.Reports;
using MFFVP.BFF.Services.Reports.DepositsReport;
using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;
using MFFVP.BFF.Services.Reports.Interfaces;
using MFFVP.BFF.Services.Reports.Strategies;
using MFFVP.BFF.Services.Reports.TechnicalSheet;
using MFFVP.BFF.Services.Reports.TechnicalSheet.Interfaces;

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

        // Registrar todas las estrategias de reportes
        services.AddScoped<DepositsReportStrategy>();
        services.AddScoped<TechnicalSheetStrategy>();
        services.AddScoped<IReportStrategyFactory, ReportStrategyFactory>();

        services.AddScoped<IDepositsReportDataProvider, DepositsReportDataProvider>();
        services.AddScoped<ITechnicalSheetReportDataProvider, TechnicalSheetReportDataProvider>();

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
        if (app is WebApplication webApp)
        {
            webApp.MapGraphQL($"/{RoutePrefix}", "BFFGateway");
        }
    }
}