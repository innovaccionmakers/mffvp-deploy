using Common.SharedKernel.Domain;
using MediatR;
using Trusts.Integrations.TrustOperations;
using Trusts.Integrations.TrustOperations.CreateTrustOperation;
using Trusts.Integrations.TrustOperations.UpdateTrustOperation;

namespace MFFVP.Api.Application.Trusts;

public interface ITrustOperationsService
{
    Task<Result<IReadOnlyCollection<TrustOperationResponse>>> GetTrustOperationsAsync(ISender sender);
    Task<Result<TrustOperationResponse>> GetTrustOperationAsync(Guid id, ISender sender);
    Task<Result> CreateTrustOperationAsync(CreateTrustOperationCommand request, ISender sender);
    Task<Result> UpdateTrustOperationAsync(UpdateTrustOperationCommand request, ISender sender);
    Task<Result> DeleteTrustOperationAsync(Guid id, ISender sender);
}