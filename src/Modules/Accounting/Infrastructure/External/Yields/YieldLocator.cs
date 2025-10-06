using Accounting.Application.Abstractions.External;
using Closing.IntegrationEvents.Yields;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Accounting.Infrastructure.External.Yields;

internal sealed class YieldLocator(IRpcClient rpc) : IYieldLocator
{
    public async Task<Result<IReadOnlyCollection<YieldResponse>>> GetAllComissionsPortfolioIdsAndClosingDate(IEnumerable<int> portfolioIds, DateTime closingDate, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetAllComissionsByPortfolioIdsAndClosingDateRequest,
            GetAllComissionsByPortfolioIdsAndClosingDateResponse>(
            new GetAllComissionsByPortfolioIdsAndClosingDateRequest(portfolioIds, closingDate),
            ct);

        return rc.IsValid
            ? Result.Success<IReadOnlyCollection<YieldResponse>>(rc.Yields.Select(y => new YieldResponse(
                YieldId: y.YieldId,
                PortfolioId: y.PortfolioId,
                Income: y.Income,
                Expenses: y.Expenses,
                Commissions: y.Commissions,
                Costs: y.Costs,
                YieldToCredit: y.YieldToCredit,
                CreditedYields: y.CreditedYields,
                ClosingDate: y.ClosingDate,
                ProcessDate: y.ProcessDate,
                IsClosed: y.IsClosed)).ToList())
            : Result.Failure<IReadOnlyCollection<YieldResponse>>(Error.Validation(rc.Code!, rc.Message!));
    }

    public async Task<Result<IReadOnlyCollection<YieldResponse>>> GetAllReturnsPortfolioIdsAndClosingDate(IEnumerable<int> portfolioIds, DateTime closingDate, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetAllReturnsByPortfolioIdsAndClosingDateRequest,
            GetAllReturnsByPortfolioIdsAndClosingDateResponse>(
            new GetAllReturnsByPortfolioIdsAndClosingDateRequest(portfolioIds, closingDate),
            ct);

        return rc.IsValid
            ? Result.Success<IReadOnlyCollection<YieldResponse>>(rc.Yields.Select(y => new YieldResponse(
                YieldId: y.YieldId,
                PortfolioId: y.PortfolioId,
                Income: y.Income,
                Expenses: y.Expenses,
                Commissions: y.Commissions,
                Costs: y.Costs,
                YieldToCredit: y.YieldToCredit,
                CreditedYields: y.CreditedYields,
                ClosingDate: y.ClosingDate,
                ProcessDate: y.ProcessDate,
                IsClosed: y.IsClosed)).ToList())
            : Result.Failure<IReadOnlyCollection<YieldResponse>>(Error.Validation(rc.Code!, rc.Message!));
    }
}
