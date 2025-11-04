using Accounting.Application.Abstractions.External;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
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
        var rcs = await rpc.CallAsync<GetOperationTypeByNameRequest, GetOperationTypeByNameResponse>(
                                                new GetOperationTypeByNameRequest(name), cancellationToken);

        var rc = rcs.OperationType?.FirstOrDefault();
        return rcs.Succeeded
            ? Result.Success((rc.OperationTypeId, EnumHelper.GetEnumMemberValue(rc.Nature), rc.Name))
            : Result.Failure<(long OperationTypeId, string Nature, string Name)>(Error.Validation(rcs.Code!, rcs.Message!));
    }
}
