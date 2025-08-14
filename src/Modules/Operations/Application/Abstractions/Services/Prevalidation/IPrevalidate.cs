using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Abstractions.Services.Prevalidation;

public record PrevalidationResult(
    (bool, int, bool) AffiliateActivation,
    ContributionRemoteData RemoteData,
    ContributionCatalogs Catalogs,
    long? BankId,
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