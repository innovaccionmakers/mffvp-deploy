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

    public async Task<Result<OperationTypeInfo>> GetOperationTypeByNameAsync(
     string name,
     CancellationToken cancellationToken)
    {

        var request = new GetOperationTypeByNameRequest(name.Trim());

        var rpcResponse = await rpcClient
            .CallAsync<GetOperationTypeByNameRequest, GetOperationTypeByNameResponse>(request, cancellationToken);

        if (!rpcResponse.Succeeded)
        {
            return Result.Failure<OperationTypeInfo>(
                Error.Validation(
                    rpcResponse.Code ?? "OPTYPE_LOOKUP_FAILED",
                    rpcResponse.Message ?? "No se obtuvo el tipo de operación por Nombre."));
        }

        var operationType = rpcResponse.OperationType.FirstOrDefault();
        if (operationType is null)
        {
            return Result.Failure<OperationTypeInfo>(
                Error.Failure("OPTYPE_PAYLOAD_NULL", "Llamada exitosa pero el payload de OperationType fue nulo."));
        }

        var info = new OperationTypeInfo(
            operationType.OperationTypeId,
            operationType.Name,
            string.IsNullOrWhiteSpace(operationType.Category) ? null : operationType.Category,
            operationType.Nature,
            operationType.Status,
            operationType.External,
            operationType.HomologatedCode
        );

        return Result.Success(info);
    }

}
