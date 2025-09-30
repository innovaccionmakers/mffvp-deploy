using Accounting.Application.Abstractions.External;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.AspNetCore.Http.HttpResults;
using Operations.IntegrationEvents.ClientOperations;
using Operations.IntegrationEvents.OperationTypes;
using Operations.Integrations.ClientOperations.GetAccountingOperations;

namespace Accounting.Infrastructure.External.Operations;

public class OperationLocator(IRpcClient rpc) : IOperationLocator
{
    public async Task<Result<IReadOnlyCollection<GetAccountingOperationsResponse>>> GetAccountingOperationsAsync(IEnumerable<int> portfolioIds,
                                                                                                                 DateTime processDate,
                                                                                                                 CancellationToken cancellationToken)
    {
        var rc = await rpc.CallAsync<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>(
                                                new GetAccountingOperationsRequestEvents(portfolioIds, processDate), cancellationToken);

        return rc.IsValid
            ? Result.Success(rc.ClientOperations)
            : Result.Failure<IReadOnlyCollection<GetAccountingOperationsResponse>>(Error.Validation(rc.Code!, rc.Message!));
    }

    public async Task<Result<(long OperationTypeId, string Nature, string Name)>> GetOperationTypeByNameAsync(string name, CancellationToken cancellationToken)
    {
        var rc = await rpc.CallAsync<GetOperationTypeByNameRequest, GetOperationTypeByNameResponse>(
                                                new GetOperationTypeByNameRequest(name), cancellationToken);

        return rc.Succeeded
            ? Result.Success((rc.OperationType!.OperationTypeId, rc.OperationType.Nature.ToString(), rc.OperationType.Name))
            : Result.Failure<(long OperationTypeId, string Nature, string Name)>(Error.Validation(rc.Code!, rc.Message!));
    }
}
