using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Closing.Application.Abstractions.Data;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    IDbConnection GetDbConnection();
}
