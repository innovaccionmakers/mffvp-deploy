using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Closing.Application.PreClosing.Services.Yield
{
    public sealed class PreclosingCleanupService : IPreclosingCleanupService
    {
        private readonly IYieldDetailRepository yieldDetailRepo;
        private readonly IYieldRepository yieldRepo;
        private readonly ILogger<PreclosingCleanupService>? logger;

        public PreclosingCleanupService(
            IYieldDetailRepository yieldDetailRepo,
            IYieldRepository yieldRepo,
            ILogger<PreclosingCleanupService>? logger = null)
        {
            this.yieldDetailRepo = yieldDetailRepo;
            this.yieldRepo = yieldRepo;
            this.logger = logger;
        }

        public async Task CleanAsync(int portfolioId, DateTime closingDateUtc, CancellationToken ct = default)
        {
            using var scope = logger?.BeginScope(new Dictionary<string, object?>
            {
                ["PortfolioId"] = portfolioId,
                ["ClosingDate"] = closingDateUtc.ToString("yyyy-MM-dd")
            });

            var sw = Stopwatch.StartNew();

            // Estos métodos usan DbContextFactory (autocommit, seguro para paralelismo)
            await yieldDetailRepo.DeleteByPortfolioAndDateAsync(portfolioId, closingDateUtc, ct);
            await yieldRepo.DeleteByPortfolioAndDateAsync(portfolioId, closingDateUtc, ct);

            sw.Stop();
            logger?.LogInformation("Preclosing cleanup OK. TimeMs={ElapsedMs}", sw.ElapsedMilliseconds);
        }
    }
}
