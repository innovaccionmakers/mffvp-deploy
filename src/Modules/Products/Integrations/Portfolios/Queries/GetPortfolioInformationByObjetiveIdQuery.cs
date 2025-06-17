namespace Products.Integrations.Portfolios.Queries;
using Common.SharedKernel.Application.Messaging;


public sealed record GetPortfolioInformationByObjetiveIdQuery(
    string ObjetiveId
    ) : IQuery<PortfolioInformationResponse>;