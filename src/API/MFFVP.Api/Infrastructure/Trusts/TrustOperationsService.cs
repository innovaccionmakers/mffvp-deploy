using Common.SharedKernel.Domain;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Trusts.Integrations.TrustOperations;
using Trusts.Integrations.TrustOperations.CreateTrustOperation;
using Trusts.Integrations.TrustOperations.DeleteTrustOperation;
using Trusts.Integrations.TrustOperations.GetTrustOperation;
using Trusts.Integrations.TrustOperations.GetTrustOperations;
using Trusts.Integrations.TrustOperations.UpdateTrustOperation;

namespace MFFVP.Api.Infrastructure.Trusts;

public sealed class TrustOperationsService : ITrustOperationsService
{
    public async Task<Result<IReadOnlyCollection<TrustOperationResponse>>> GetTrustOperationsAsync(ISender sender)
    {
        return await sender.Send(new GetTrustOperationsQuery());
    }

    public async Task<Result<TrustOperationResponse>> GetTrustOperationAsync(Guid id, ISender sender)
    {
        return await sender.Send(new GetTrustOperationQuery(id));
    }

    public async Task<Result> CreateTrustOperationAsync(CreateTrustOperationCommand request, ISender sender)
    {
        return await sender.Send(request);
    }

    public async Task<Result> UpdateTrustOperationAsync(UpdateTrustOperationCommand request, ISender sender)
    {
        return await sender.Send(request);
    }

    public async Task<Result> DeleteTrustOperationAsync(Guid id, ISender sender)
    {
        return await sender.Send(new DeleteTrustOperationCommand(id));
    }
}