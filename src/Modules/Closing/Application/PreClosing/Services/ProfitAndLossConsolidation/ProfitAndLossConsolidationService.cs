
using Closing.Domain.ProfitLosses;

namespace Closing.Application.PreClosing.Services.ProfitAndLossConsolidation
{
    public class ProfitAndLossConsolidationService : IProfitAndLossConsolidationService
    {
        IProfitLossRepository _profitLossRepository;
        public ProfitAndLossConsolidationService(
            IProfitLossRepository profitLossRepository)
        {
            _profitLossRepository = profitLossRepository;
        }

        public async Task<IReadOnlyList<ProfitLossConceptSummary>> GetProfitAndLossSummaryAsync(int portfolioId, DateTime closingDate, bool isClosing = false)
        {
            var conceptSummaries = await _profitLossRepository
                .GetConceptSummaryAsync(portfolioId, closingDate);
            return conceptSummaries;
        }

    }
}
