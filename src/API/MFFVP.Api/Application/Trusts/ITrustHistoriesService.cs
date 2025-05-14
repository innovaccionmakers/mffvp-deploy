using Common.SharedKernel.Domain;
using MediatR;

using Trusts.Integrations.TrustHistories;
using Trusts.Integrations.TrustHistories.CreateTrustHistory;
using Trusts.Integrations.TrustHistories.UpdateTrustHistory;

namespace MFFVP.Api.Application.Trusts
{
    public interface ITrustHistoriesService
    {
        Task<Result<IReadOnlyCollection<TrustHistoryResponse>>> GetTrustHistoriesAsync(ISender sender);
        Task<Result<TrustHistoryResponse>> GetTrustHistoryAsync(long trustHistoryId, ISender sender);
        Task<Result> CreateTrustHistoryAsync(CreateTrustHistoryCommand request, ISender sender);
        Task<Result> UpdateTrustHistoryAsync(UpdateTrustHistoryCommand request, ISender sender);
        Task<Result> DeleteTrustHistoryAsync(long trustHistoryId, ISender sender);
    }
}