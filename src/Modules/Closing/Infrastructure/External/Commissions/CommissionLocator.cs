
using Closing.Application.Abstractions.External.Commissions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Closing.Infrastructure.External.Commissions;
internal sealed class CommissionLocator(ICapRpcClient capRpcClient) : ICommissionLocator
{
    public async Task<Result<IReadOnlyCollection<GetCommissionsByPortfolioIdResponse>>> GetActiveCommissionsAsync(
        int portfolioId,
        CancellationToken cancellationToken)
    {
        var request = new CommissionsByPortfolioRequest(portfolioId);

        var response = await capRpcClient.CallAsync<
            CommissionsByPortfolioRequest,
            CommissionsByPortfolioResponse>(
            nameof(CommissionsByPortfolioRequest),
            request,
            timeout: TimeSpan.FromSeconds(5),
            cancellationToken
        );

        return response.Succeeded
            ? Result.Success(response.Commissions!)
            : Result.Failure<IReadOnlyCollection<GetCommissionsByPortfolioIdResponse>>(
                Error.Validation(response.Code!, response.Message!));
    }
}