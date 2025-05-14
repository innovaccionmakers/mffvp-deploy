using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.TrustHistories;
using Trusts.Integrations.TrustHistories;
using Trusts.Integrations.TrustHistories.GetTrustHistories;

namespace Trusts.Application.TrustHistories.GetTrustHistories;

internal sealed class GetTrustHistoriesQueryHandler(
    ITrustHistoryRepository trusthistoryRepository)
    : IQueryHandler<GetTrustHistoriesQuery, IReadOnlyCollection<TrustHistoryResponse>>
{
    public async Task<Result<IReadOnlyCollection<TrustHistoryResponse>>> Handle(GetTrustHistoriesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await trusthistoryRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new TrustHistoryResponse(
                e.TrustHistoryId,
                e.TrustId,
                e.Earnings,
                e.Date,
                e.SalesUserId))
            .ToList();

        return Result.Success<IReadOnlyCollection<TrustHistoryResponse>>(response);
    }
}