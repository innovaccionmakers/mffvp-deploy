
using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.Queries;

public sealed record  GetPortfolioInfoForClosingQuery(int PortfolioId) : IQuery<PortfolioInfoForClosingResponse>;
