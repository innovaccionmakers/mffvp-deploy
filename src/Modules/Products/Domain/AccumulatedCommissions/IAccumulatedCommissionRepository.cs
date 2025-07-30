
namespace Products.Domain.AccumulatedCommissions;

public interface IAccumulatedCommissionRepository
{
    Task<AccumulatedCommission?> GetByPortfolioAndCommissionAsync(
        int portfolioId,
        int commissionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(AccumulatedCommission commission, CancellationToken cancellationToken);
}