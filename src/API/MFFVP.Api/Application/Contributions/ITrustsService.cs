using Common.SharedKernel.Domain;
using MediatR;

using Contributions.Integrations.Trusts;
using Contributions.Integrations.Trusts.CreateTrust;
using Contributions.Integrations.Trusts.UpdateTrust;

namespace MFFVP.Api.Application.Contributions
{
    public interface ITrustsService
    {
        Task<Result<IReadOnlyCollection<TrustResponse>>> GetTrustsAsync(ISender sender);
        Task<Result<TrustResponse>> GetTrustAsync(Guid id, ISender sender);
        Task<Result> CreateTrustAsync(CreateTrustCommand request, ISender sender);
        Task<Result> UpdateTrustAsync(UpdateTrustCommand request, ISender sender);
        Task<Result> DeleteTrustAsync(Guid id, ISender sender);
    }
}