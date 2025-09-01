using Common.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Reports.BalancesAndMovements;
using Reports.Application.Reports.Strategies;
using Reports.Domain.BalancesAndMovements;
using Reports.Infrastructure.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
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
        services.AddScoped<BalancesAndMovementsReport>();
        services.AddScoped<IReportsExperienceQueries, ReportsExperienceQueries>(); 
        services.AddScoped<IReportStrategy, BalancesAndMovementsReport>();
        services.AddScoped<IReportStrategyFactory, ReportStrategyFactory>();
        services.AddScoped<IBalancesAndMovementsReportRepository, BalancesAndMovementsReportRepository>();
        services.AddTransient<IReportsDbConnectionFactory, ReportsDbConnectionFactory>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        
    }
}
