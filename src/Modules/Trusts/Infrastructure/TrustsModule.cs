using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trusts.Application.Abstractions.Data;
using Trusts.Application.Abstractions.Lookups;
using Trusts.Application.Abstractions.Rules;
using Trusts.Domain.Clients;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.InputInfos;
using Trusts.Domain.Portfolios;
using Trusts.Domain.TrustOperations;
using Trusts.Domain.Trusts;
using Trusts.Infrastructure.Database;
using Trusts.Infrastructure.Mocks;
using Trusts.Infrastructure.RulesEngine;

namespace Trusts.Infrastructure;

public static class TrustsModule
{
    public static IServiceCollection AddTrustsModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine(opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrustsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("TrustsDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Trusts)
                );
        });

        services.AddScoped<ITrustRepository, TrustRepository>();
        services.AddScoped<ICustomerDealRepository, CustomerDealRepository>();
        services.AddScoped<IInputInfoRepository, InputInfoRepository>();
        services.AddScoped<ITrustOperationRepository, TrustOperationRepository>();
        services.AddScoped<ILookupService, InMemoryLookupService>();
        services.AddScoped<IPortfolioRepository, InMemoryPortfolioRepository>();
        services.AddScoped<IClientRepository, InMemoryClientRepository>();
        services.AddSingleton<IErrorCatalog, InMemoryErrorCatalog>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TrustsDbContext>());
    }
}