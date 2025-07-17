using Common.SharedKernel.Application.Messaging;

namespace Products.Integrations.AdditionalInformation;

public sealed record GetAdditionalInformationQuery(
    int AffiliateId,
    IReadOnlyCollection<(int ObjectiveId, int PortfolioId)> Pairs)
    : IQuery<IReadOnlyCollection<AdditionalInformationItem>>;
