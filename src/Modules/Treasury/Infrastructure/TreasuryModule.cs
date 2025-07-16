using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Treasury.Application.Abstractions.Data;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Domain.TreasuryMovements;
using Treasury.Infrastructure.BankAccounts;
using Treasury.Infrastructure.Database;
using Treasury.Infrastructure.Issuers;
using Treasury.Infrastructure.TreasuryConcepts;
using Treasury.Infrastructure.TreasuryMovements;
using Treasury.IntegrationEvents.TreasuryMovements.TreasuryMovementsByPortfolio;
using Common.SharedKernel.Application.Rpc;

namespace Treasury.Infrastructure;

public class TreasuryModule : IModuleConfiguration
{
    public string ModuleName => "Treasury";
    public string RoutePrefix => "api/treasury";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("TreasuryDatabase");

        services.AddDbContext<TreasuryDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Treasury)
                );
        });

        services.AddScoped<IIssuerRepository, IssuerRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ITreasuryConceptRepository, TreasuryConceptRepository>();
        services.AddScoped<ITreasuryMovementRepository, TreasuryMovementRepository>();
        services.AddScoped<IRpcHandler<TreasuryMovementsByPortfolioRequest, TreasuryMovementsByPortfolioResponse>, TreasuryMovementsByPortfolioConsumer>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TreasuryDbContext>());

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        if (app is WebApplication webApp)
        {
            // webApp.MapTreasuryEndpoints();
        }
    }
}