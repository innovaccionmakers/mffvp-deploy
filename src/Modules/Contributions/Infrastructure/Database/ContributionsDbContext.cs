
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Contributions.Application.Abstractions.Data;
using Contributions.Domain.Trusts;
using Contributions.Infrastructure.Trusts;
using Contributions.Domain.ClientOperations;
using Contributions.Infrastructure.ClientOperations;
using Contributions.Domain.TrustOperations;
using Contributions.Infrastructure.TrustOperations;

namespace Contributions.Infrastructure.Database;

public sealed class ContributionsDbContext(DbContextOptions<ContributionsDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Trust> Trusts { get; set; }
    internal DbSet<ClientOperation> ClientOperations { get; set; }
    internal DbSet<TrustOperation> TrustOperations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Contributions);

        modelBuilder.ApplyConfiguration(new TrustConfiguration());
        modelBuilder.ApplyConfiguration(new ClientOperationConfiguration());
        modelBuilder.ApplyConfiguration(new TrustOperationConfiguration());
    }

    public async Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (Database.CurrentTransaction is not null)
        {
            await Database.CurrentTransaction.DisposeAsync();
        }

        return (await Database.BeginTransactionAsync(cancellationToken)).GetDbTransaction();
    }
}