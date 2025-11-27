using Accounting.Application.Abstractions.External;
using Closing.IntegrationEvents.YieldsToDistribute;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Accounting.Infrastructure.External.YieldsToDistribute;

internal sealed class YieldToDistributeLocator(IRpcClient rpc) : IYieldToDistributeLocator
{
    public async Task<Result<IReadOnlyCollection<DistributedYieldGroupResponse>>> GetDistributedYieldGroupResponse(IEnumerable<int> portfolioIds, DateTime closingDate, string concept, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetDistributedYieldGroupByConceptRequest,
            GetDistributedYieldGroupByConceptResponse>(
            new GetDistributedYieldGroupByConceptRequest(portfolioIds, closingDate, concept),
            ct);

        return rc.IsValid
            ? Result.Success<IReadOnlyCollection<DistributedYieldGroupResponse>>(rc.DistributedYieldGroups.Select(d => new DistributedYieldGroupResponse(
                ClosinDate: d.ClosinDate,
                PortfolioId: d.PortofolioId,
                Concept: d.Concept,
                TotalYieldAmount: d.TotalYieldAmount)).ToList())
            : Result.Failure<IReadOnlyCollection<DistributedYieldGroupResponse>>(Error.Validation(rc.Code!, rc.Message!));
    }
}
