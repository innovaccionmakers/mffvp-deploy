using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountProcess
{
    internal sealed class AccountProcessHandler(
        ISender sender,
        IClosingExecutionStore closingValidator) : ICommandHandler<AccountProcessCommand, string>
    {
        public async Task<Result<string>> Handle(AccountProcessCommand command, CancellationToken cancellationToken)
        {
            var isActive = await closingValidator.IsClosingActiveAsync(cancellationToken);
            if (isActive)
                return Result.Failure<string>(new Error("0001", "Existe un proceso de cierre activo.", ErrorType.Validation));

            var deleteCommand = new DeleteAccountingAssistantsCommand();
            var deleteResult = await sender.Send(deleteCommand, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Failure<string>(deleteResult.Error);

            var acountingOperationsCommand = new AccountingOperationsCommand(command.PortfolioIds, command.ProcessDate);
            var resultAcountingOperations = await sender.Send(acountingOperationsCommand, cancellationToken);
            if (resultAcountingOperations.IsFailure)
                return Result.Failure<string>(resultAcountingOperations.Error);

            var accountingConceptsCommand = new AccountingConceptsCommand(command.PortfolioIds, command.ProcessDate);
            var resultAccountingConcepts = await sender.Send(accountingConceptsCommand, cancellationToken);
            if (resultAccountingConcepts.IsFailure)
                return Result.Failure<string>(resultAccountingConcepts.Error);

            return Result.Success<string>(string.Empty);
        }
    }
}
