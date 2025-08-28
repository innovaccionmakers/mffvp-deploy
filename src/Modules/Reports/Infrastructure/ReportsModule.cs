using Common.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Balances;
using Reports.Application.Strategies;
using Reports.Presentation.GraphQL;

namespace Reports.Infrastructure;

public class ReportsModule : IModuleConfiguration
{
    public string ModuleName => "Reports";
    public string RoutePrefix => "api/reports";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Servicios específicos de reportes
        // TODO: Agregar servicios cuando se definan los requerimientos
        services.AddScoped<BalancesReport>();
        services.AddScoped<IReportsExperienceQueries, ReportsExperienceQueries>(); 
        services.AddScoped<IReportStrategy, BalancesReport>();
        services.AddScoped<IReportStrategyFactory, ReportStrategyFactory>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        
    }
}
