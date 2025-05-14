
using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.CreatePortfolio;
using Products.Integrations.Portfolios.DeletePortfolio;
using Products.Integrations.Portfolios.GetPortfolio;
using Products.Integrations.Portfolios.GetPortfolios;
using Products.Integrations.Portfolios.UpdatePortfolio;

using MFFVP.Api.Application.Products;

namespace MFFVP.Api.Infrastructure.Products
{
    public sealed class PortfoliosService : IPortfoliosService
    {
        public async Task<Result<IReadOnlyCollection<PortfolioResponse>>> GetPortfoliosAsync(ISender sender)
        {
            return await sender.Send(new GetPortfoliosQuery());
        }

        public async Task<Result<PortfolioResponse>> GetPortfolioAsync(long portfolioId, ISender sender)
        {
            return await sender.Send(new GetPortfolioQuery(portfolioId));
        }

        public async Task<Result> CreatePortfolioAsync(CreatePortfolioCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdatePortfolioAsync(UpdatePortfolioCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeletePortfolioAsync(long portfolioId, ISender sender)
        {
            return await sender.Send(new DeletePortfolioCommand(portfolioId));
        }
    }
}