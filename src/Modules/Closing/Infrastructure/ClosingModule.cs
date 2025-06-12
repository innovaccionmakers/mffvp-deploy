using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Infrastructure.ConfigurationParameters;
using Closing.Infrastructure.ProfitLossConcepts;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.External.Portfolios;
using Closing.Infrastructure.ProfitLosses;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Closing.Infrastructure;

public static class ClosingModule
{
    public static IServiceCollection AddClosingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<ClosingModuleMarker>(typeof(ClosingModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ClosingDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("ClosingDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Closing)
                );
        });

        services.AddScoped<IProfitLossConceptRepository, ProfitLossConceptRepository>();
        services.AddScoped<IProfitLossRepository, ProfitLossRepository>();
        services.AddScoped<IErrorCatalog<ClosingModuleMarker>, ErrorCatalog>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();


        services.AddScoped<IPortfolioValidator, PortfolioValidator>();
        
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClosingDbContext>());
    }
}