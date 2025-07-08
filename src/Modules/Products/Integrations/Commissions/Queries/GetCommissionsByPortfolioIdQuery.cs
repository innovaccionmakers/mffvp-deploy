using Common.SharedKernel.Application.Messaging;
using Products.Integrations.Commissions.Response;

namespace Products.Integrations.Commissions.Queries;
public sealed record GetCommissionsByPortfolioIdQuery(
    int PortfolioId
    ) : IQuery<IReadOnlyCollection<GetCommissionsByPortfolioIdResponse>>;