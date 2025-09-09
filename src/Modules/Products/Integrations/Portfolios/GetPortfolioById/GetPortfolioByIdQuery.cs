using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.GetPortfolioById;

public sealed record GetPortfolioByIdQuery(
    string CodigoPortafolio
) : IQuery<GetPortfolioByIdResponse>;