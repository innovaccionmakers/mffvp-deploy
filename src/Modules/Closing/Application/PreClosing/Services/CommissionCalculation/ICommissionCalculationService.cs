using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.CommissionCalculation;

public interface ICommissionCalculationService
{
    Task<Result<decimal>> CalculateAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}
