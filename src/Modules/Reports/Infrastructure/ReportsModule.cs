using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Reports.Strategies;
using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.LoadingInfo.Contracts;
using Reports.Application.LoadingInfo.Audit;
using Reports.Application.LoadingInfo.Features.Balances;
using Reports.Application.LoadingInfo.Features.Closing;
using Reports.Application.LoadingInfo.Features.People;
using Reports.Application.LoadingInfo.Features.Products;
using Reports.Application.LoadingInfo.Orchestration;
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
using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.Balances;
using Reports.Domain.LoadingInfo.Closing;
using Reports.Domain.LoadingInfo.People;
using Reports.Domain.LoadingInfo.Products;
using Reports.Domain.TechnicalSheet;
using Reports.Domain.TransmissionFormat;
using Reports.Infrastructure.BalancesAndMovements;
using Reports.Infrastructure.Configuration;
using Reports.Infrastructure.ConnectionFactory;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using Reports.Infrastructure.Database;
using Reports.Infrastructure.Deposits;
using Reports.Infrastructure.Health;
using Reports.Infrastructure.LoadingExecutions;
using Reports.Infrastructure.LoadingInfo.Balances;
using Reports.Infrastructure.LoadingInfo.Closing;
using Reports.Infrastructure.LoadingInfo.People;
using Reports.Infrastructure.LoadingInfo.Products;
using Reports.Infrastructure.Options;
using Reports.Infrastructure.TechnicalSheet;
using Reports.Infrastructure.TransmissionFormat;
using Reports.IntegrationEvents.LoadingInfo;
using Reports.Presentation.GraphQL;


namespace Reports.Infrastructure;

public class ReportsModule : IModuleConfiguration
{
    public string ModuleName => "Reports";
    public string RoutePrefix => "api/reports";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("ReportsWriteDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            var commandTimeoutSeconds = configuration.GetValue<int?>("CustomSettings:DatabaseTimeouts:CommandTimeoutSeconds") ?? 30;
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region, commandTimeoutSeconds).GetAwaiter().GetResult();
        }

        // Web/API (1 DbContext por request con tracking)
        services.AddDbContextPool<ReportsWriteDbContext>(options =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                   .UseNpgsql(connectionString, npgsqlOptions =>
                       npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Reports));
        });

        // Workers/Paralelo (crear contextos a demanda)
        services.AddPooledDbContextFactory<ReportsWriteDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Reports)
                );
        });

        services.Configure<ReportsOptions>(configuration.GetSection("Reports"));
        services.Configure<DatabaseTimeoutsOptions>(
        configuration.GetSection(DatabaseTimeoutsOptions.SectionName));

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
        services.AddScoped<BalancesAndMovementsReport>();
        services.AddScoped<IBalancesAndMovementsReportRepository, BalancesAndMovementsReportRepository>();
        services.AddScoped<IReportStrategy, BalancesAndMovementsReport>();
        services.AddScoped<DepositsReport>();
        services.AddScoped<IDepositsRepository, DepositsRepository>();
        services.AddScoped<IReportStrategy, DepositsReport>();



        services.AddScoped<DailyDataConsumer>();
        services.AddScoped<IReportsExperienceMutations, ReportsExperienceMutations>();

        services.AddTransient<IReportsDbReadConnectionFactory, ReportsDbReadConnectionFactory>();
        services.AddScoped<IEtlAuditRunner, EtlAuditRunner>();
        services.AddScoped<IEtlExecutionService, EtlExecutionService>();
        services.AddScoped<IEtlExecutionRepository, EtlExecutionRepository>();

        // Orquestador
        services.AddScoped<ILoadingInfoOrchestrator, LoadingInfoOrchestrator>();

        // ETL Loaders 
        services.AddScoped<IPeopleLoader, EtlClientMembershipService>();
        services.AddScoped<IProductsLoader, EtlProductsService>();
        services.AddScoped<IClosingLoader, EtlClosingService>();
        services.AddScoped<IBalancesLoader, EtlBalancesService>();

        // Repos ETL afiliados
        services.AddScoped<IPeopleSheetReadRepository, PeopleSheetReadRepository>();
        services.AddScoped<IPeopleSheetWriteRepository, PeopleSheetWriteRepository>();

        // Repos ETL productos
        services.AddScoped<IProductSheetReadRepository, ProductSheetReadRepository>();
        services.AddScoped<IProductSheetWriteRepository, ProductSheetWriteRepository>();

        // Repos ETL Cierre
        services.AddScoped<IClosingSheetReadRepository, ClosingSheetReadRepository>();
        services.AddScoped<IClosingSheetWriteRepository, ClosingSheetWriteRepository>();

        // Repos ETL Saldos 
        services.AddScoped<IBalanceSheetReadRepository, BalanceSheetReadRepository>();
        services.AddScoped<IBalanceSheetWriteRepository, BalanceSheetWriteRepository>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {

    }
}
