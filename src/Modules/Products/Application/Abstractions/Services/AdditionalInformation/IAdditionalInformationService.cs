using Products.Integrations.AdditionalInformation;

namespace Products.Application.Abstractions.Services.AdditionalInformation;

public interface IAdditionalInformationService
{
    Task<IReadOnlyCollection<AdditionalInformationItem>> GetInformationAsync(
        IReadOnlyCollection<(int ObjectiveId, int PortfolioId)> pairs,
        CancellationToken ct = default);
}
