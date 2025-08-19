using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.Channels;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Operations.Domain.OriginModes;
using Operations.Domain.Origins;
using Operations.Domain.OperationTypes;
using Operations.Domain.TrustOperations;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.TemporaryClientOperations;
using Operations.Infrastructure.TemporaryAuxiliaryInformations;
using Operations.Infrastructure.Channels;
using Operations.Infrastructure.OriginModes;
using Operations.Infrastructure.Origins;
using Operations.Infrastructure.OperationTypes;
using Operations.Infrastructure.TrustOperations;

namespace Operations.Infrastructure.Database;

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ClientOperation> ClientOperations { get; set; }
    internal DbSet<AuxiliaryInformation> AuxiliaryInformations { get; set; }
    internal DbSet<TemporaryClientOperation> TemporaryClientOperations { get; set; }
    internal DbSet<TemporaryAuxiliaryInformation> TemporaryAuxiliaryInformations { get; set; }
    internal DbSet<OperationType> OperationTypes { get; set; }
    internal DbSet<TrustOperation> TrustOperations { get; set; }
    internal DbSet<Origin> Origins { get; set; }
    internal DbSet<Channel> Channels { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }
    internal DbSet<OriginMode> OriginModes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Operations);

        modelBuilder.ApplyConfiguration(new ClientOperationConfiguration());
        modelBuilder.ApplyConfiguration(new AuxiliaryInformationConfiguration());
        modelBuilder.ApplyConfiguration(new TemporaryClientOperationConfiguration());
        modelBuilder.ApplyConfiguration(new TemporaryAuxiliaryInformationConfiguration());
        modelBuilder.ApplyConfiguration(new OperationTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TrustOperationConfiguration());
        modelBuilder.ApplyConfiguration(new OriginConfiguration());
        modelBuilder.ApplyConfiguration(new ChannelConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Operations));
        modelBuilder.ApplyConfiguration(new OriginModeConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}