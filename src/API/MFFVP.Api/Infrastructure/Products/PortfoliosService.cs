
using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.GetPortfolio;
using Products.Integrations.Portfolios.GetPortfolios;

using MFFVP.Api.Application.Products;

namespace MFFVP.Api.Infrastructure.Products
{
    public sealed class PortfoliosService : IPortfoliosService
    {
        public async Task<Result<PortfolioResponse>> GetPortfolioAsync(int portfolioId, ISender sender)
        {
            return await sender.Send(new GetPortfolioQuery(portfolioId));
        }
        
        public async Task<Result<IReadOnlyCollection<PortfolioResponse>>> GetPortfoliosAsync(ISender sender)
        {
            return await sender.Send(new GetPortfoliosQuery());
        }
    }
}