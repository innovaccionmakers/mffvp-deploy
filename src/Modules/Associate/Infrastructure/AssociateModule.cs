using Application.EventBus;
using Associate.Application.Abstractions.Data;
using Associate.Application.Abstractions.Rules;
using Associate.Domain.Activates;
using Associate.Domain.ConfigurationParameters;
using Associate.Infrastructure.Database;
using Associate.Infrastructure.RulesEngine;
using Common.SharedKernel.Infrastructure.Configuration;
using Infrastructure.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Associate.Infrastructure;

public static class ActivatesModule
{
    public static IServiceCollection AddActivatesModule(this IServiceCollection services,
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
        services.AddDbContext<AssociateDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("AssociateDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Associate)
                );
        });

        services.AddScoped<IActivateRepository, ActivateRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IErrorCatalog, ErrorCatalog>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AssociateDbContext>());

        services.AddScoped<PersonRequestReplyService>();
    }
}