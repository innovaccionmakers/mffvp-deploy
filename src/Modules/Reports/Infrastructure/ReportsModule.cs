using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Helpers.Finance;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Reports.Common.Strategies;
using Reports.Application.Reports.DailyClosing;
using Reports.Application.Reports.DailyClosing.Strategies;
using Reports.Application.Strategies;
using Reports.Application.TechnicalSheet;
using Reports.Domain.DailyClosing;
using Reports.Domain.Health;
using Reports.Domain.TechnicalSheet;
using Reports.Infrastructure.ConnectionFactory;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using Reports.Infrastructure.DailyClosing;
using Reports.Infrastructure.Health;
using Reports.Infrastructure.Options;
using Reports.Infrastructure.TechnicalSheet;
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
        services.AddTransient<ITechnicalSheetRepository, TechnicalSheetRepository>();
        services.AddScoped<IReportGeneratorStrategy, DailyClosingReportStrategy>();
        services.AddScoped<IReportStrategyBuilder, ReportStrategyBuilder>();
        services.AddScoped<IDailyClosingReportBuilder, DailyClosingReportBuilder>();
        services.AddScoped<DailyClosingReport>();        
        services.AddScoped<TechnicalSheetReport>();
        services.AddScoped<IReportStrategy, DailyClosingReport>();      
        services.AddScoped<IReportStrategy, TechnicalSheetReport>();
        services.AddScoped<IReportStrategyFactory, ReportStrategyFactory>();
        services.AddScoped<IReportsExperienceQueries, ReportsExperienceQueries>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

    }
}
