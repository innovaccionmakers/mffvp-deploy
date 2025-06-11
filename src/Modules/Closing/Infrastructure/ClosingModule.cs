using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Infrastructure.ProfitLossConcepts;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.ProfitLosses;
using Common.SharedKernel.Infrastructure.Configuration;
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
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClosingDbContext>());
    }
}