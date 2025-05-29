using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Portfolios;

namespace MFFVP.Api.Application.Products
{
    public interface IPortfoliosService
    {
        Task<Result<PortfolioResponse>> GetPortfolioAsync(int portfolioId, ISender sender);
    }
}