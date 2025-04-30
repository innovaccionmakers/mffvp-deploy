using Common.SharedKernel.Domain;
using MediatR;

using Contributions.Integrations.TrustOperations;
using Contributions.Integrations.TrustOperations.CreateTrustOperation;
using Contributions.Integrations.TrustOperations.UpdateTrustOperation;

namespace MFFVP.Api.Application.Contributions
{
    public interface ITrustOperationsService
    {
        Task<Result<IReadOnlyCollection<TrustOperationResponse>>> GetTrustOperationsAsync(ISender sender);
        Task<Result<TrustOperationResponse>> GetTrustOperationAsync(Guid id, ISender sender);
        Task<Result> CreateTrustOperationAsync(CreateTrustOperationCommand request, ISender sender);
        Task<Result> UpdateTrustOperationAsync(UpdateTrustOperationCommand request, ISender sender);
        Task<Result> DeleteTrustOperationAsync(Guid id, ISender sender);
    }
}