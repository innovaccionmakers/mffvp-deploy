using Accounting.Application.Abstractions.External;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Operations.IntegrationEvents.ClientOperations;
using Common.SharedKernel.Core.Primitives;
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
}
