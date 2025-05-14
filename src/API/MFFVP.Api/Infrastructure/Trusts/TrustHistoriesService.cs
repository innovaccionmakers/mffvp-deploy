
using Common.SharedKernel.Domain;
using MediatR;

using Trusts.Integrations.TrustHistories;
using Trusts.Integrations.TrustHistories.CreateTrustHistory;
using Trusts.Integrations.TrustHistories.DeleteTrustHistory;
using Trusts.Integrations.TrustHistories.GetTrustHistory;
using Trusts.Integrations.TrustHistories.GetTrustHistories;
using Trusts.Integrations.TrustHistories.UpdateTrustHistory;

using MFFVP.Api.Application.Trusts;

namespace MFFVP.Api.Infrastructure.Trusts
{
    public sealed class TrustHistoriesService : ITrustHistoriesService
    {
        public async Task<Result<IReadOnlyCollection<TrustHistoryResponse>>> GetTrustHistoriesAsync(ISender sender)
        {
            return await sender.Send(new GetTrustHistoriesQuery());
        }

        public async Task<Result<TrustHistoryResponse>> GetTrustHistoryAsync(long trustHistoryId, ISender sender)
        {
            return await sender.Send(new GetTrustHistoryQuery(trustHistoryId));
        }

        public async Task<Result> CreateTrustHistoryAsync(CreateTrustHistoryCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateTrustHistoryAsync(UpdateTrustHistoryCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteTrustHistoryAsync(long trustHistoryId, ISender sender)
        {
            return await sender.Send(new DeleteTrustHistoryCommand(trustHistoryId));
        }
    }
}