using Closing.Domain.ProfitLosses;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Closing.Application.PreClosing.Services.ProfitAndLoss
{
    public class ProfitAndLossConsolidationService : IProfitAndLossConsolidationService
    {
        private readonly IProfitLossQueryRepository profitLossQueries;
        private readonly ILogger<ProfitAndLossConsolidationService>? logger;

        public ProfitAndLossConsolidationService(
            IProfitLossQueryRepository profitLossQueries,
            ILogger<ProfitAndLossConsolidationService>? logger = null)
        {
            this.profitLossQueries = profitLossQueries;
            this.logger = logger;
        }

        public async Task<IReadOnlyList<ProfitLossConceptSummary>> GetProfitAndLossSummaryAsync(
            int portfolioId,
            DateTime closingDate,
            CancellationToken cancellationToken = default)
        {
            var sw = Stopwatch.StartNew();

            var summaries = await profitLossQueries
                .GetReadOnlyConceptSummaryAsync(portfolioId, closingDate, cancellationToken);

            sw.Stop();
            logger?.LogInformation(
                "PyG summary obtenido. PortfolioId={PortfolioId}, Date={Date}, Registros={Count}, TimeMs={ElapsedMs}",
                portfolioId, closingDate.ToString("yyyy-MM-dd"), summaries.Count, sw.ElapsedMilliseconds);

            return summaries;
        }
    }
}
