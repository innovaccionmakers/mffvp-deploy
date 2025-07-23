using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.Abort;

public interface IAbortClosingService
{
    Task<Result> AbortAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}