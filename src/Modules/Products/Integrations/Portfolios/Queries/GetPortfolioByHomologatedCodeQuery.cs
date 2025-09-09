using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.Queries;

public sealed record GetPortfolioByHomologatedCodeQuery(string HomologatedCode) : IQuery<PortfolioResponse>;
