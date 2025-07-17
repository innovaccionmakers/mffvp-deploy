namespace Products.IntegrationEvents.AdditionalInformation;

public sealed record GetAdditionalInformationRequest(
    int AffiliateId,
    IReadOnlyCollection<(int ObjectiveId, int PortfolioId)> Pairs);
