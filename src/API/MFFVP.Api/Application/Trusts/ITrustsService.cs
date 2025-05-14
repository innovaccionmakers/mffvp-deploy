using Common.SharedKernel.Domain;
using MediatR;

using Trusts.Integrations.Trusts;
using Trusts.Integrations.Trusts.CreateTrust;
using Trusts.Integrations.Trusts.UpdateTrust;

namespace MFFVP.Api.Application.Trusts
{
    public interface ITrustsService
    {
        Task<Result<IReadOnlyCollection<TrustResponse>>> GetTrustsAsync(ISender sender);
        Task<Result<TrustResponse>> GetTrustAsync(long trustId, ISender sender);
        Task<Result> CreateTrustAsync(CreateTrustCommand request, ISender sender);
        Task<Result> UpdateTrustAsync(UpdateTrustCommand request, ISender sender);
        Task<Result> DeleteTrustAsync(long trustId, ISender sender);
    }
}