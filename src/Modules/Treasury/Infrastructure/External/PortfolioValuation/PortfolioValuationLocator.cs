using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Treasury.Application.Abstractions.External;
using Closing.IntegrationEvents.PortfolioValuation;
namespace Treasury.Infrastructure.External.PortfolioValuation;

internal sealed class PortfolioValuationLocator(IRpcClient rpc) : IPortfolioValuationLocator
{
    public async Task<Result<bool>> CheckPortfolioValuationExists(DateTime closingDate, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            CheckPortfolioValuationExistsRequest,
            CheckPortfolioValuationExistsResponse>(
            new CheckPortfolioValuationExistsRequest(closingDate),
            ct);
        return rc.Succeeded
            ? Result.Success(rc.Exists)
            : Result.Failure<bool>(Error.Validation(rc.Code, rc.Message));

    }
}
