using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Reports.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Reports.BalancesAndMovements;
using Reports.Application.Reports.Common.Strategies;
using Reports.Application.Reports.Deposits;
using Reports.Application.Reports.Strategies;
using Reports.Application.Reports.TransmissionFormat;
using Reports.Application.Reports.TransmissionFormat.Strategies;
using Reports.Application.TechnicalSheet;
using Reports.Domain.BalancesAndMovements;
using Reports.Domain.Deposits;
using Reports.Domain.Health;
using Reports.Domain.TechnicalSheet;
using Reports.Domain.TransmissionFormat;
using Reports.Infrastructure.BalancesAndMovements;
using Reports.Infrastructure.ConnectionFactory;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using Reports.Infrastructure.Deposits;
using Reports.Infrastructure.Health;
using Reports.Infrastructure.Options;
using Reports.Infrastructure.TechnicalSheet;
using Reports.Infrastructure.TransmissionFormat;
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
        services.AddTransient<ITransmissionFormatReportRepository, TransmissionFormatReportRepository>();
        services.AddTransient<ITechnicalSheetRepository, TechnicalSheetRepository>();
        services.AddScoped<IReportGeneratorStrategy, TransmissionFormatReportStrategy>();
        services.AddScoped<IReportStrategyBuilder, ReportStrategyBuilder>();
        services.AddScoped<ITransmissionFormatReportBuilder, TransmissionFormatReportBuilder>();
        services.AddScoped<TransmissionFormatReport>();        
        services.AddScoped<TechnicalSheetReport>();
        services.AddScoped<IReportStrategy, TransmissionFormatReport>();      
        services.AddScoped<IReportStrategy, TechnicalSheetReport>();
        services.AddScoped<IReportStrategyFactory, ReportStrategyFactory>();
        services.AddScoped<IReportsExperienceQueries, ReportsExperienceQueries>();
        services.AddTransient<IReportsDbConnectionFactory, ReportsDbConnectionFactory>();
        services.AddScoped<BalancesAndMovementsReport>();
        services.AddScoped<IBalancesAndMovementsReportRepository, BalancesAndMovementsReportRepository>();
        services.AddScoped<IReportStrategy, BalancesAndMovementsReport>();
        services.AddScoped<DepositsReport>();
        services.AddScoped<IDepositsRepository, DepositsRepository>();
        services.AddScoped<IReportStrategy, DepositsReport>();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

    }
}
