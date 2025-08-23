using Closing.Application.PreClosing.Services.Commission.Dto;

namespace Closing.Application.PreClosing.Services.Commission.Interfaces;

public interface ICommissionCalculationService
{
    Task<IReadOnlyList<CommissionConceptSummary>> CalculateAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken = default);
}
