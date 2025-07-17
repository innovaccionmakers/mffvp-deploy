using Closing.Domain.Commission;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.CommissionCalculation;

public interface ICommissionCalculationService
{
    Task<IReadOnlyList<CommissionConceptSummary>> CalculateAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}
