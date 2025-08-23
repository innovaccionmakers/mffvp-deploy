using Closing.Application.Abstractions.External.Operations.OperationTypes;

using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.IntegrationEvents.OperationTypes;

namespace Closing.Infrastructure.External.Operations.OperationTypes;

internal sealed class OperationTypesLocator(IRpcClient rpcClient) : IOperationTypesLocator
{
    public async Task<Result<IReadOnlyCollection<OperationTypeInfo>>> GetAllOperationTypesAsync(
        CancellationToken cancellationToken)
    {
        var request = new GetAllOperationTypesRequest();

        var response = await rpcClient.CallAsync<
            GetAllOperationTypesRequest,
            GetAllOperationTypesResponse>(
            request,
            cancellationToken
        );

        IReadOnlyCollection<OperationTypeInfo>? types = response.Types?.Select(c => new OperationTypeInfo(
            OperationTypeId: c.OperationTypeId,
            Name: c.Name,
            Category: c.Category,
            Nature: c.Nature,
            Status: c.Status,
            External: c.External,
            HomologatedCode: c.HomologatedCode
        )).ToList();
        return response.Succeeded
            ? Result.Success(types!)
            : Result.Failure<IReadOnlyCollection<OperationTypeInfo>>(
                Error.Validation(response.Code!, response.Message!));
    }
}
