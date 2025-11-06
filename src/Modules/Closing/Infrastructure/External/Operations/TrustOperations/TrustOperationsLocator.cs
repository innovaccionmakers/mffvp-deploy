using System.Linq;

using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.PreClosing.Services.ExtraReturns.Dto;

using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.IntegrationEvents.TrustOperations;

namespace Closing.Infrastructure.External.Operations.TrustOperations;

internal sealed class TrustOperationsLocator(
    IRpcClient rpcClient,
    IOperationTypesLocator operationTypesLocator)
    : ITrustOperationsLocator
{
    private const string ExtraReturnOperationTypeName = "Ajuste Rendimientos";

    public async Task<Result<IReadOnlyCollection<TrustOperationRemoteResponse>>> GetTrustOperationsAsync(
        int portfolioId,
        DateTime processDateUtc,
        CancellationToken cancellationToken)
    {
        var operationTypeResult = await operationTypesLocator
            .GetOperationTypeByNameAsync(ExtraReturnOperationTypeName, cancellationToken);

        if (operationTypeResult.IsFailure)
        {
            return Result.Failure<IReadOnlyCollection<TrustOperationRemoteResponse>>(operationTypeResult.Error);
        }

        var operationType = operationTypeResult.Value;

        var request = new GetTrustOperationsByPortfolioProcessDateAndTypeRequest(
            portfolioId,
            processDateUtc,
            operationType.OperationTypeId);

        var response = await rpcClient.CallAsync<
            GetTrustOperationsByPortfolioProcessDateAndTypeRequest,
            GetTrustOperationsByPortfolioProcessDateAndTypeResponse>(
            request,
            cancellationToken);

        if (!response.Succeeded)
        {
            return Result.Failure<IReadOnlyCollection<TrustOperationRemoteResponse>>(
                Error.Validation(
                    response.Code ?? "TRUST_OPERATIONS_RPC_FAILURE",
                    response.Message ?? "Fallo al consultar operaciones de fideicomiso."));
        }

        var operations = response.Operations?
            .Select(operation => new TrustOperationRemoteResponse(
                operation.PortfolioId,
                operation.ProcessDate,
                operation.OperationType.Id,
                operation.OperationType.Name,
                operation.Value))
            .ToList()
            ?? new List<TrustOperationRemoteResponse>();

        return Result.Success<IReadOnlyCollection<TrustOperationRemoteResponse>>(operations);
    }
}
