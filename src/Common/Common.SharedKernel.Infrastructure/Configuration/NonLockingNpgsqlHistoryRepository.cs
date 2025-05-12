using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations.Internal;

namespace Common.SharedKernel.Infrastructure.Configuration;
#pragma warning disable EF1001
public class NonLockingNpgsqlHistoryRepository : NpgsqlHistoryRepository
{
    public NonLockingNpgsqlHistoryRepository(HistoryRepositoryDependencies dependencies)
        : base(dependencies)
    {
    }

    public override IMigrationsDatabaseLock AcquireDatabaseLock()
    {
        return new NoopMigrationsDatabaseLock(this);
    }

    public override Task<IMigrationsDatabaseLock> AcquireDatabaseLockAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IMigrationsDatabaseLock>(new NoopMigrationsDatabaseLock(this));
    }

    private class NoopMigrationsDatabaseLock : IMigrationsDatabaseLock
    {
        private readonly NpgsqlHistoryRepository _historyRepository;

        public NoopMigrationsDatabaseLock(NpgsqlHistoryRepository historyRepository)
        {
            _historyRepository = historyRepository;
        }

        public IHistoryRepository HistoryRepository => _historyRepository;

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}