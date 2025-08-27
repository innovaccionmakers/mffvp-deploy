
namespace Closing.Application.PreClosing.Services.Yield.Interfaces;

public interface IPreclosingCleanupService
{
    Task CleanAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken = default);
}