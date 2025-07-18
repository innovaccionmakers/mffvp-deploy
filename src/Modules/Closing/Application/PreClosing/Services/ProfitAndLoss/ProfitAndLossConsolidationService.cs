
using Closing.Domain.ProfitLosses;

namespace Closing.Application.PreClosing.Services.ProfitAndLoss
{
    public class ProfitAndLossConsolidationService : IProfitAndLossConsolidationService
    {
        IProfitLossRepository _profitLossRepository;
        public ProfitAndLossConsolidationService(
            IProfitLossRepository profitLossRepository)
        {
            _profitLossRepository = profitLossRepository;
        }

        public async Task<IReadOnlyList<ProfitLossConceptSummary>> GetProfitAndLossSummaryAsync(
             int portfolioId,
             DateTime closingDate)
        {
            var summaries = await _profitLossRepository
                .GetConceptSummaryAsync(portfolioId, closingDate);

            return summaries;
        }


    }
}
