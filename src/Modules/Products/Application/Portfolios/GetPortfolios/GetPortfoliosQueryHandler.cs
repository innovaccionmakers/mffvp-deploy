using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios.GetPortfolios;
using Products.Integrations.Portfolios;
using System.Collections.Generic;
using System.Linq;

namespace Products.Application.Portfolios.GetPortfolios;

internal sealed class GetPortfoliosQueryHandler(
    IPortfolioRepository portfolioRepository)
    : IQueryHandler<GetPortfoliosQuery, IReadOnlyCollection<PortfolioResponse>>
{
    public async Task<Result<IReadOnlyCollection<PortfolioResponse>>> Handle(GetPortfoliosQuery request, CancellationToken cancellationToken)
    {
        var entities = await portfolioRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new PortfolioResponse(
                e.PortfolioId,
                e.StandardCode,
                e.Name,
                e.ShortName,
                e.ModalityId,
                e.InitialMinimumAmount))
            .ToList();

        return Result.Success<IReadOnlyCollection<PortfolioResponse>>(response);
    }
}