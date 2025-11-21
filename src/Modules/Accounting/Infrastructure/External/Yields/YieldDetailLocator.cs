using Accounting.Application.Abstractions.External;
using Closing.IntegrationEvents.Yields;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Accounting.Infrastructure.External.Yields;

internal sealed class YieldDetailLocator(IRpcClient rpc) : IYieldDetailsLocator
{
    public async Task<Result<IReadOnlyCollection<YieldDetailResponse>>> GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(IEnumerable<int> portfolioIds, string source, DateTime closingDate, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetYieldsDetailsByPortfolioIdsClosingDateAndSourceRequest,
            GetYieldsDetailsByPortfolioIdsClosingDateAndSourceResponse>(
            new GetYieldsDetailsByPortfolioIdsClosingDateAndSourceRequest(portfolioIds, closingDate, source),
            ct);

        return rc.IsValid
            ? Result.Success<IReadOnlyCollection<YieldDetailResponse>>(rc.YieldDetails.Select(yd => new YieldDetailResponse(
                YieldDetailId: yd.YieldDetailId,
                PortfolioId: yd.PortfolioId,
                Income: yd.Income,
                Expenses: yd.Expenses,
                Commissions: yd.Commissions,
                ClosingDate: yd.ClosingDate,
                ProcessDate: yd.ProcessDate,
                IsClosed: yd.IsClosed)).ToList())
            : Result.Failure<IReadOnlyCollection<YieldDetailResponse>>(Error.Validation(rc.Code!, rc.Message!));
    }
}

