using Associate.Application.Abstractions.Data;
using Associate.Domain.Activates;
using Associate.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.Configuration;
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
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ActivatesDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("AssociateDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Associate)
                ).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IActivateRepository, ActivateRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ActivatesDbContext>());
    }
}