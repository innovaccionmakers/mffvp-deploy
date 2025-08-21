
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
             DateTime closingDate, 
             CancellationToken cancellationToken = default)
        {
            var summaries = await _profitLossRepository
                .GetReadOnlyConceptSummaryAsync(portfolioId, closingDate, cancellationToken);

            return summaries;
        }


    }
}
