using Accounting.Application.Abstractions.External;
using Closing.IntegrationEvents.YieldsToDistribute;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Accounting.Infrastructure.External.YieldsToDistribute;

internal sealed class YieldToDistributeLocator(IRpcClient rpc) : IYieldToDistributeLocator
{
    public async Task<Result<IReadOnlyCollection<GenericDebitNoteResponse>>> GetDistributedYieldGroupResponse(IEnumerable<int> portfolioIds, DateTime closingDate, string concept, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetDistributedYieldGroupByConceptRequest,
            GetDistributedYieldGroupByConceptResponse>(
            new GetDistributedYieldGroupByConceptRequest(portfolioIds, closingDate, concept),
            ct);

        return rc.IsValid
            ? Result.Success<IReadOnlyCollection<GenericDebitNoteResponse>>(rc.DistributedYieldGroups.Select(d => new GenericDebitNoteResponse(
                ClosinDate: d.ClosinDate,
                PortfolioId: d.PortofolioId,
                Value: d.TotalYieldAmount)).ToList())
            : Result.Failure<IReadOnlyCollection<GenericDebitNoteResponse>>(Error.Validation(rc.Code!, rc.Message!));
    }
}
