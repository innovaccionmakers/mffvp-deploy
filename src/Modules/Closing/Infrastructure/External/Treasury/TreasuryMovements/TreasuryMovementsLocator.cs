using Closing.Application.Abstractions.External.Treasury.TreasuryMovements;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Treasury.IntegrationEvents.TreasuryMovements.TreasuryMovementsByPortfolio;

namespace Closing.Infrastructure.External.Treasury.TreasuryMovements;

internal sealed class TreasuryMovementsLocator(ICapRpcClient capRpcClient) : ITreasuryMovementsLocator
{
    public async Task<Result<IReadOnlyCollection<MovementsByPortfolioRemoteResponse>>> GetMovementsByPortfolioAsync(
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

        IReadOnlyCollection<MovementsByPortfolioRemoteResponse>? movements = response.movements?.Select(c => new MovementsByPortfolioRemoteResponse(
          c.ConceptId,
          c.ConceptName,
          c.Nature.ToString(),
          c.AllowsExpense,
          c.TotalAmount)).ToList();
        return response.Succeeded
            ? Result.Success(movements!)
            : Result.Failure<IReadOnlyCollection<MovementsByPortfolioRemoteResponse>>(
                Error.Validation(response.Code!, response.Message!));
    }
}
