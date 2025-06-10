using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.Channels;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Domain.SubtransactionTypes;
using Operations.Domain.TrustWithdrawals;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.Channels;
using Operations.Infrastructure.Origins;
using Operations.Infrastructure.SubtransactionTypes;
using Operations.Infrastructure.TrustWithdrawals;

namespace Operations.Infrastructure.Database;

public sealed class OperationsDbContext(DbContextOptions<OperationsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<ClientOperation> ClientOperations { get; set; }
    internal DbSet<AuxiliaryInformation> AuxiliaryInformations { get; set; }
    internal DbSet<SubtransactionType> SubtransactionTypes { get; set; }
    internal DbSet<TrustWithdrawalOperation> TrustWithdrawalOperations { get; set; }
    internal DbSet<Origin> Origins { get; set; }
    internal DbSet<Channel> Channels { get; set; }
    internal DbSet<ConfigurationParameter> ConfigurationParameters { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Operations);

        modelBuilder.ApplyConfiguration(new ClientOperationConfiguration());
        modelBuilder.ApplyConfiguration(new AuxiliaryInformationConfiguration());
        modelBuilder.ApplyConfiguration(new SubtransactionTypeConfiguration());
        modelBuilder.ApplyConfiguration(new TrustWithdrawalOperationConfiguration());
        modelBuilder.ApplyConfiguration(new OriginConfiguration());
        modelBuilder.ApplyConfiguration(new ChannelConfiguration());
        modelBuilder.ApplyConfiguration(new ConfigurationParameterConfiguration(Schemas.Operations));
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null) await Database.CurrentTransaction.DisposeAsync();

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}