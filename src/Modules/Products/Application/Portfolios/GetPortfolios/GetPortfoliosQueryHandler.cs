using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.GetPortfolios;

namespace Products.Application.Portfolios.GetPortfolios;

internal sealed class GetPortfoliosQueryHandler(
    IPortfolioRepository repository)
    : IQueryHandler<GetPortfoliosQuery, IReadOnlyCollection<PortfolioResponse>>
{
    public async Task<Result<IReadOnlyCollection<PortfolioResponse>>> Handle(
        GetPortfoliosQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(p => new PortfolioResponse(
                p.PortfolioId,
                p.HomologatedCode,
                p.Name,
                p.ShortName,
                p.ModalityId,
                p.InitialMinimumAmount,
                p.CurrentDate))
            .ToList();

        return Result.Success<IReadOnlyCollection<PortfolioResponse>>(response);
    }
}