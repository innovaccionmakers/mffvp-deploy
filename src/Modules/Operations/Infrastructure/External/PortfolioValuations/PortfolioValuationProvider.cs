using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Closing.IntegrationEvents.PortfolioValuation;

namespace Operations.Infrastructure.External.PortfolioValuations;

internal sealed class PortfolioValuationProvider(IRpcClient rpcClient) : IPortfolioValuationProvider
{
    public async Task<Result<decimal>> GetUnitValueAsync(
        int portfolioId,
        DateTime closingDate,
        CancellationToken cancellationToken)
    {
        var normalizedDate = DateTime.SpecifyKind(closingDate, DateTimeKind.Utc);

        var response = await rpcClient.CallAsync<
            GetPortfolioValuationInfoRequest,
            GetPortfolioValuationInfoResponse>(
            new GetPortfolioValuationInfoRequest(portfolioId, normalizedDate),
            cancellationToken);

        if (response.IsValid && response.ValuationInfo is not null)
        {
            return Result.Success(response.ValuationInfo.UnitValue);
        }

        return Result.Failure<decimal>(
            Error.Validation(
                response.Code ?? string.Empty,
                response.Message ?? string.Empty));
    }
}
