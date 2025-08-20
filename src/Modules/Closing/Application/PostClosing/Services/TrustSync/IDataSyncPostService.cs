using Common.SharedKernel.Domain;

namespace Closing.Application.PostClosing.Services.TrustSync;

public interface IDataSyncPostService
{
    Task<Result> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}