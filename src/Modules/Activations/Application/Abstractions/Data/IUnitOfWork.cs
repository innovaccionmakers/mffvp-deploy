using System.Data.Common;

namespace Activations.Application.Abstractions.Data
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task<DbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    }
}