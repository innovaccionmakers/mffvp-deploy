
using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.GetPortfolio;

using MFFVP.Api.Application.Products;

namespace MFFVP.Api.Infrastructure.Products
{
    public sealed class PortfoliosService : IPortfoliosService
    {
        public async Task<Result<PortfolioResponse>> GetPortfolioAsync(int portfolioId, ISender sender)
        {
            return await sender.Send(new GetPortfolioQuery(portfolioId));
        }
    }
}