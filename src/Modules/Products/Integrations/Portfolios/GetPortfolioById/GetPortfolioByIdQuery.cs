using Common.SharedKernel.Application.Messaging;
using Products.Integrations.Portfolios.GetPortfolioById;

namespace Products.Integrations.Portfolios.Queries;

public sealed record GetPortfolioByIdQuery(
    string CodigoPortafolio
) : IQuery<GetPortfolioByIdResponse>;