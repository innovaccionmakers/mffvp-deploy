namespace Products.Integrations.Portfolios.Queries;
using Common.SharedKernel.Application.Messaging;
using Products.Domain.Portfolios;

public sealed record class GetPortfolioInformationByObjetiveIdQuery(
    string ObjetiveId
    ) : IQuery<PortfolioInformation>;