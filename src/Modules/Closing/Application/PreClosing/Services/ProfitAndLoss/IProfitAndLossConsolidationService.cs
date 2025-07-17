
using Closing.Domain.ProfitLosses;

namespace Closing.Application.PreClosing.Services.ProfitAndLossConsolidation
{
    public interface IProfitAndLossConsolidationService
    {
        public Task<IReadOnlyList<ProfitLossConceptSummary>> GetProfitAndLossSummaryAsync(int portfolioId, DateTime closingDate);
    }
}
