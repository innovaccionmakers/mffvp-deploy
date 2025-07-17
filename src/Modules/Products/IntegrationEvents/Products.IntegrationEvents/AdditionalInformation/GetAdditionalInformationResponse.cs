using Products.Integrations.AdditionalInformation;

namespace Products.IntegrationEvents.AdditionalInformation;

public sealed record GetAdditionalInformationResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<AdditionalInformationItem> Items);
