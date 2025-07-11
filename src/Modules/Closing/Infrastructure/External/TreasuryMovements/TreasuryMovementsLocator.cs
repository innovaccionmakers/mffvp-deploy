using Closing.Application.Abstractions.External.TreasuryMovements;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Closing.Infrastructure.External.TreasuryMovements;

internal sealed class TreasuryMovementsLocator(ICapRpcClient capRpcClient) : ITreasuryMovementsLocator
{
    public async Task<Result<IReadOnlyCollection<GetMovementsByPortfolioIdResponse>>> GetMovementsByPortfolioAsync(
       int portfolioId,
       DateTime closingDate,
       CancellationToken cancellationToken)
    {
        var request = new TreasuryMovementsByPortfolioRequest(portfolioId, closingDate);

        var response = await capRpcClient.CallAsync<
            TreasuryMovementsByPortfolioRequest,
            TreasuryMovementsByPortfolioResponse>(
            nameof(TreasuryMovementsByPortfolioRequest),
            request,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken
        );

        return response.Succeeded
            ? Result.Success(response.movements!)
            : Result.Failure<IReadOnlyCollection<GetMovementsByPortfolioIdResponse>>(
                Error.Validation(response.Code!, response.Message!));
    }
}
