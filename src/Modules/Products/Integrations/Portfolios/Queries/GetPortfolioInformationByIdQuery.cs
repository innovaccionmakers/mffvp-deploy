using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.Queries;

public sealed record class GetPortfolioInformationByIdQuery(int PortfolioId) : IQuery<CompletePortfolioInformationResponse>;
