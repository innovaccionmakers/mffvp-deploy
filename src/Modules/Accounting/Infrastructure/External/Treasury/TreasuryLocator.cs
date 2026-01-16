using Accounting.Application.Abstractions.External;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Treasury.IntegrationEvents.Issuers.GetIssuersByIds;

namespace Accounting.Infrastructure.External.Treasury;

public class TreasuryLocator(IRpcClient rpc) : ITreasuryLocator
{
    public async Task<Result<IReadOnlyCollection<IssuerResponse>>> GetIssuersByIdsAsync(
        IEnumerable<long> ids,
        CancellationToken cancellationToken)
    {
        var rc = await rpc.CallAsync<GetIssuersByIdsRequest, GetIssuersByIdsResponse>(
            new GetIssuersByIdsRequest(ids), cancellationToken);

        return rc.IsValid
            ? Result.Success(rc.Issuers)
            : Result.Failure<IReadOnlyCollection<IssuerResponse>>(
                Error.Validation(rc.Code ?? string.Empty, rc.Message ?? string.Empty));
    }
}

