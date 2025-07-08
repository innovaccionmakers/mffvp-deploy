using Products.Domain.Portfolios;

namespace Products.Domain.Commissions
{
    public interface ICommissionRepository
    {
        Task<IReadOnlyCollection<Commission>> GetActiveCommissionsByPortfolioAsync (int portfolioId, CancellationToken cancellationToken = default);
    }
}
