using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.Configuration;
using Operations.Domain.ConfigurationParameters;
using Operations.Infrastructure.ConfigurationParameters;

namespace Operations.Infrastructure;

public static class OperationsModule
{
    public static IServiceCollection AddOperationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OperationsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("OperationsDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Operations)
                );
        });

        services.AddScoped<IClientOperationRepository, ClientOperationRepository>();
        services.AddScoped<IAuxiliaryInformationRepository, AuxiliaryInformationRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OperationsDbContext>());
    }
}