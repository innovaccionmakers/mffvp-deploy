using Closing.Application.PreClosing.Services.ProfitAndLoss.Dto;
using Closing.Domain.ProfitLosses;

namespace Closing.Application.PreClosing.Services.ProfitAndLoss
{
    public interface IProfitAndLossConsolidationService
    {
        Task<IReadOnlyList<ProfitLossConceptSummary>> GetProfitAndLossSummaryAsync(
             int portfolioId,
             DateTime closingDate,
             CancellationToken cancellationToken = default);
    }
}
