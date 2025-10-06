using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Accounting.Application.AccountProcess
{
    internal sealed class AccountProcessHandler(
        IServiceScopeFactory serviceScopeFactory,
        IClosingExecutionStore closingValidator) : ICommandHandler<AccountProcessCommand>
    {
        public async Task<Result> Handle(AccountProcessCommand command, CancellationToken cancellationToken)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var scopedMediator = scope.ServiceProvider.GetRequiredService<ISender>();

            var isActive = await closingValidator.IsClosingActiveAsync(cancellationToken);
            if (isActive)
                return Result.Failure(new Error("0001", "Existe un proceso de cierre activo.", ErrorType.Validation));

            var deleteCommand = new DeleteAccountingAssistantsCommand();
            var deleteResult = await scopedMediator.Send(deleteCommand, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Failure(deleteResult.Error);

            var acountingOperationsCommand = new AccountingOperationsCommand(command.PortfolioIds, command.ProcessDate);
            var resultAcountingOperations = await scopedMediator.Send(acountingOperationsCommand, cancellationToken);
            if (resultAcountingOperations.IsFailure)
                return Result.Failure(resultAcountingOperations.Error);

            var accountingConceptsCommand = new AccountingConceptsCommand(command.PortfolioIds, command.ProcessDate);
            var resultAccountingConcepts = await scopedMediator.Send(accountingConceptsCommand, cancellationToken);
            if (resultAccountingConcepts.IsFailure)
                return Result.Failure(resultAccountingConcepts.Error);

            return Result.Success();
        }
    }
}
