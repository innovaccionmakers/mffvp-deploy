using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.CreatePortfolio;
using Products.Integrations.Portfolios.UpdatePortfolio;

namespace MFFVP.Api.Application.Products
{
    public interface IPortfoliosService
    {
        Task<Result<IReadOnlyCollection<PortfolioResponse>>> GetPortfoliosAsync(ISender sender);
        Task<Result<PortfolioResponse>> GetPortfolioAsync(int portfolioId, ISender sender);
        Task<Result> CreatePortfolioAsync(CreatePortfolioCommand request, ISender sender);
        Task<Result> UpdatePortfolioAsync(UpdatePortfolioCommand request, ISender sender);
        Task<Result> DeletePortfolioAsync(long portfolioId, ISender sender);
    }
}