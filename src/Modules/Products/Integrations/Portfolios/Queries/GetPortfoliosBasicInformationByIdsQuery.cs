using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.Portfolios.Queries;

public sealed record GetPortfoliosBasicInformationByIdsQuery(IEnumerable<int> PortfolioIds)
    : IQuery<IReadOnlyCollection<PortfolioBasicInformationResponse>>;

public sealed record PortfolioBasicInformationResponse(
    int PortfolioId,
    string NitApprovedPortfolio,
    int VerificationDigit,
    string Name
);

