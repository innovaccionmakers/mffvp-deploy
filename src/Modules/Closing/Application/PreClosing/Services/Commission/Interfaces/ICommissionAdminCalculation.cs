using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Commission.Interfaces
{
    public interface ICommissionAdminCalculation
    {
        Task<Result<decimal>> CalculateAsync(int portfolioId, DateTime closingDate, decimal commissionPercentage, CancellationToken ct);
    }
}
