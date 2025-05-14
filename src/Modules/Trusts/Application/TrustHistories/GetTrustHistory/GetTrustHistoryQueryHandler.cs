using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.TrustHistories;
using Trusts.Integrations.TrustHistories;
using Trusts.Integrations.TrustHistories.GetTrustHistory;

namespace Trusts.Application.TrustHistories.GetTrustHistory;

internal sealed class GetTrustHistoryQueryHandler(
    ITrustHistoryRepository trusthistoryRepository)
    : IQueryHandler<GetTrustHistoryQuery, TrustHistoryResponse>
{
    public async Task<Result<TrustHistoryResponse>> Handle(GetTrustHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var trusthistory = await trusthistoryRepository.GetAsync(request.TrustHistoryId, cancellationToken);
        if (trusthistory is null)
            return Result.Failure<TrustHistoryResponse>(TrustHistoryErrors.NotFound(request.TrustHistoryId));
        var response = new TrustHistoryResponse(
            trusthistory.TrustHistoryId,
            trusthistory.TrustId,
            trusthistory.Earnings,
            trusthistory.Date,
            trusthistory.SalesUserId
        );
        return response;
    }
}