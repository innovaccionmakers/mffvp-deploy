using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.IntegrationEvents.ClientOperations;

namespace Accounting.Application.AccountProcess
{
    internal sealed class AccountProcessHandle(
        IClosingExecutionStore closingValidator,
        IRpcClient rpcClient) : ICommandHandler<AccountProcessCommand, string>
    {
        public async Task<Result<string>> Handle(AccountProcessCommand command, CancellationToken cancellationToken)
        {
            var isActive = await closingValidator.IsClosingActiveAsync(cancellationToken);
            if (isActive)
                return Result.Failure<string>(new Error("0001", "Existe un proceso de cierre activo.", ErrorType.Validation));

            var personValidation = await rpcClient.CallAsync<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>(
                                                            new GetAccountingOperationsRequestEvents(command.PortfolioIds, command.ProcessDate), cancellationToken);
            if (!personValidation.IsValid)
                return Result.Failure<string>(Error.Validation(personValidation.Code ?? string.Empty, personValidation.Message ?? string.Empty));

            return Result.Success<string>(string.Empty);
        }
    }
}
