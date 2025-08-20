using Closing.Application.Abstractions.External.Operations.OperationTypes;

using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.IntegrationEvents.OperationTypes;

namespace Closing.Infrastructure.External.Operations.OperationTypes;

internal sealed class OperationTypesLocator(IRpcClient rpcClient) : IOperationTypesLocator
{
    public async Task<Result<IReadOnlyCollection<OperationTypesRemoteResponse>>> GetAllOperationTypesAsync(
        CancellationToken cancellationToken)
    {
        var request = new GetAllOperationTypesRequest();

        var response = await rpcClient.CallAsync<
            GetAllOperationTypesRequest,
            GetAllOperationTypesResponse>(
            request,
            cancellationToken
        );

        IReadOnlyCollection<OperationTypesRemoteResponse>? types = response.Types?.Select(c => new OperationTypesRemoteResponse(
            OperationTypeId: c.OperationTypeId,
            Name: c.Name,
            Category: c.Category,
            Nature: c.Nature,
            Status: c.Status
        )).ToList();
        return response.Succeeded
            ? Result.Success(types!)
            : Result.Failure<IReadOnlyCollection<OperationTypesRemoteResponse>>(
                Error.Validation(response.Code!, response.Message!));
    }
}
