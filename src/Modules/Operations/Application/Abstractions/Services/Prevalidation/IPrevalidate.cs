using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Domain.Banks;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Abstractions.Services.Prevalidation;

public record PrevalidationResult(
    (bool, int, bool) AffiliateActivation,
    ContributionRemoteData RemoteData,
    ContributionCatalogs Catalogs,
    Bank? Bank,
    bool IsFirstContribution,
    bool DocumentTypeExists,
    bool AffiliateFound
);

public interface IPrevalidate
{
    Task<Result<PrevalidationResult>> ValidateAsync(
        CreateContributionCommand command,
        CancellationToken cancellationToken);
} 