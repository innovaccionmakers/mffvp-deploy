using Accounting.Application.Abstractions.External;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Treasury.IntegrationEvents.Issuers.GetIssuersByIds;

namespace Accounting.Infrastructure.External.Treasury;

public class TreasuryLocator(IRpcClient rpc) : ITreasuryLocator
{
    public async Task<Result<IReadOnlyCollection<IssuerInfo>>> GetIssuersByIdsAsync(
        IEnumerable<long> ids,
        CancellationToken cancellationToken)
    {
        var rc = await rpc.CallAsync<GetIssuersByIdsRequest, GetIssuersByIdsResponse>(
            new GetIssuersByIdsRequest(ids), cancellationToken);

        if (!rc.IsValid)
        {
            return Result.Failure<IReadOnlyCollection<IssuerInfo>>(
                Error.Validation(rc.Code ?? string.Empty, rc.Message ?? string.Empty));
        }

        var issuerInfos = rc.Issuers.Select(issuer => new IssuerInfo(
            issuer.Id,
            issuer.Nit,
            issuer.Digit,
            issuer.Description
        )).ToList();

        return Result.Success<IReadOnlyCollection<IssuerInfo>>(issuerInfos);
    }
}

