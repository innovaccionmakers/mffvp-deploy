

using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.CommissionCalculation
{
    public interface ICommissionAdminCalculationService
    {
        Task<Result<decimal>> CalculateAsync(int portfolioId, DateTime closingDate, decimal commissionPercentage, CancellationToken ct);
    }
}
