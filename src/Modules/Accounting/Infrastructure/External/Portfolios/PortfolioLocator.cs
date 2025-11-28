using Accounting.Application.Abstractions.External;
using Products.IntegrationEvents.Portfolio.AreAllPortfoliosClosed;
using Products.IntegrationEvents.Portfolio.GetPortfolioInformation;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Accounting.Infrastructure.External.Portfolios;

public class PortfolioLocator(IRpcClient rpc) : IPortfolioLocator
{
    public async Task<Result<PortfolioResponse>> GetPortfolioInformationAsync(int portfolioId, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetPortfolioInformationByIdRequest,
            GetPortfolioInformationByIdResponse>(
            new GetPortfolioInformationByIdRequest(portfolioId),
            ct);

        return rc.Succeeded
            ? Result.Success(new PortfolioResponse(rc.PortfolioInformation!.NitApprovedPortfolio, rc.PortfolioInformation.VerificationDigit, rc.PortfolioInformation.Name))
            : Result.Failure<PortfolioResponse>(Error.Validation(rc.Code!, rc.Message!));
    }

    public async Task<Result<bool>> AreAllPortfoliosClosedAsync(IEnumerable<int> portfolioIds, DateTime date, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            AreAllPortfoliosClosedRequest,
            AreAllPortfoliosClosedResponse>(
            new AreAllPortfoliosClosedRequest(portfolioIds, date),
            ct);

        return rc.Succeeded
            ? Result.Success(rc.AreAllClosed)
            : Result.Failure<bool>(Error.Validation(rc.Code!, rc.Message!));
    }
}
