using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.ConfigurationParameters;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.ConfigurationParameters;

namespace Operations.Infrastructure.Database;

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ClientOperation> ClientOperations { get; set; }
    internal DbSet<AuxiliaryInformation> AuxiliaryInformations { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Operations);

        modelBuilder.ApplyConfiguration(new ClientOperationConfiguration());
        modelBuilder.ApplyConfiguration(new AuxiliaryInformationConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}