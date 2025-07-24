using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.TrustSync;

public interface IDataSyncService
{
    Task<Result> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}