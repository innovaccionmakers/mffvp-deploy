using Common.SharedKernel.Domain;
using Treasury.IntegrationEvents.Issuers.GetIssuersByIds;

namespace Accounting.Application.Abstractions.External;

public interface ITreasuryLocator
{
    Task<Result<IReadOnlyCollection<IssuerResponse>>> GetIssuersByIdsAsync(
        IEnumerable<long> ids,
        CancellationToken cancellationToken);
}

