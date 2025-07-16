using Closing.Application.Abstractions.External.Products.Commissions;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Products.IntegrationEvents.Commission.CommissionsByPortfolio;

namespace Closing.Infrastructure.External.Products.Commissions;
internal sealed class CommissionLocator(IRpcClient rpcClient) : ICommissionLocator
{
    public async Task<Result<IReadOnlyCollection<CommissionsByPortfolioRemoteResponse>>> GetActiveCommissionsAsync(
        int portfolioId,
        CancellationToken cancellationToken)
    {
        var request = new CommissionsByPortfolioRequest(portfolioId);

        var response = await rpcClient.CallAsync<
            CommissionsByPortfolioRequest,
            CommissionsByPortfolioResponse>(
            request,
            cancellationToken
        );

        IReadOnlyCollection<CommissionsByPortfolioRemoteResponse>? commissions = response.Commissions?.Select(c => new CommissionsByPortfolioRemoteResponse(
            c.CommissionId,
            c.Concept,
            c.Modality,
            c.CommissionType,
            c.Period,
            c.CalculationBase,
            c.CalculationRule)).ToList();
        return response.Succeeded
            ? Result.Success(commissions!)
            : Result.Failure<IReadOnlyCollection<CommissionsByPortfolioRemoteResponse>>(
                Error.Validation(response.Code!, response.Message!));
    }
}