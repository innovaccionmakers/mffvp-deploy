using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Helpers.Finance;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Reports.BalancesAndMovements;
using Reports.Application.Reports.Common.Strategies;
using Reports.Application.Reports.DailyClosing;
using Reports.Application.Reports.DailyClosing.Strategies;
using Reports.Application.Reports.Strategies;
using Reports.Domain.BalancesAndMovements;
using Reports.Domain.DailyClosing;
using Reports.Domain.Health;
using Reports.Infrastructure.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using Reports.Infrastructure.DailyClosing;
using Reports.Infrastructure.Health;
using Reports.Infrastructure.Options;
using Reports.Presentation.GraphQL;

namespace Reports.Infrastructure;

public class ReportsModule : IModuleConfiguration
{
    public string ModuleName => "Reports";
    public string RoutePrefix => "api/reports";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ReportsOptions>(configuration.GetSection("Reports"));
        services.AddTransient<IReportsDbConnectionFactory, ReportsDbConnectionFactory>();
        services.AddTransient<IProfitabilityCalculator, ProfitabilityCalculator>();
        services.AddTransient<IReportHealthRepository, ReportHealthRepository>();
        services.AddTransient<IDailyClosingReportRepository, DailyClosingReportRepository>();
        services.AddScoped<IReportGeneratorStrategy, DailyClosingReportStrategy>();
        services.AddScoped<IReportStrategyBuilder, ReportStrategyBuilder>();
        services.AddScoped<IDailyClosingReportBuilder, DailyClosingReportBuilder>();
        services.AddScoped<DailyClosingReport>();
        services.AddScoped<IReportStrategyFactory, ReportStrategyFactory>();
        services.AddScoped<IReportsExperienceQueries, ReportsExperienceQueries>();
        services.AddTransient<IReportsDbConnectionFactory, ReportsDbConnectionFactory>();
        services.AddScoped<BalancesAndMovementsReport>();
        services.AddScoped<IBalancesAndMovementsReportRepository, BalancesAndMovementsReportRepository>();
        services.AddScoped<IReportStrategy, BalancesAndMovementsReport>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        
    }
}
