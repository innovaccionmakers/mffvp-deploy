using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.Integrations.ClientOperations.GetAccountingOperations;
using System.Collections.Concurrent;

namespace Accounting.Application.AccountingOperations
{
    internal sealed class AccountingOperationsHandler(
        ISender sender,
        ILogger<AccountingOperationsHandler> logger,
        AccountingOperationsHandlerValidation validator,
        IOperationLocator operationLocator,
        IInconsistencyHandler inconsistencyHandler) : ICommandHandler<AccountingOperationsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var contributionResult = await ProcessContributionOperationsAsync(command, cancellationToken);
                if (contributionResult.IsFailure)
                    return contributionResult;

                var debitNoteResult = await ProcessDebitNoteOperationsAsync(command, cancellationToken);
                if (debitNoteResult.IsFailure)
                    return debitNoteResult;

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar las operaciones contables para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                     command.ProcessDate,
                     string.Join(",", command.PortfolioIds)
                 );
                return Result.Failure<bool>(Error.Problem("Exception", "Ocurrio un error inesperado al procesar las operaciones contables"));
            }
        }

        private async Task<Result<bool>> ProcessContributionOperationsAsync(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            var operations = await operationLocator.GetAccountingOperationsAsync(
                command.PortfolioIds,
                command.ProcessDate,
                cancellationToken);

            return await ProcessOperationsByTypeAsync(
                operations,
                command,
                OperationTypeAttributes.Names.Contribution,
                cancellationToken);
        }

        private async Task<Result<bool>> ProcessDebitNoteOperationsAsync(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            var operations = await operationLocator.GetAccountingDebitNoteOperationsAsync(
                command.PortfolioIds,
                command.ProcessDate,
                cancellationToken);

            return await ProcessOperationsByTypeAsync(
                operations,
                command,
                OperationTypeAttributes.Names.DebitNote,
                cancellationToken);
        }

        private async Task<Result<bool>> ProcessOperationsByTypeAsync(
            Result<IReadOnlyCollection<GetAccountingOperationsResponse>> operationsResult,
            AccountingOperationsCommand command,
            string operationTypeName,
            CancellationToken cancellationToken)
        {
            if (!operationsResult.IsSuccess)
                return Result.Failure<bool>(Error.Validation(operationsResult.Error.Code ?? string.Empty, operationsResult.Error.Description ?? string.Empty));

            if (operationsResult.Value.Count == 0)
            {
                logger.LogInformation("No hay operaciones de tipo {OperationType} para procesar", operationTypeName);
                return Result.Success(true);
            }

            var errors = new ConcurrentBag<AccountingInconsistency>();
            var operationsByPortfolio = operationsResult.Value.GroupBy(op => op.PortfolioId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var portfolioId in command.PortfolioIds)
            {
                if (!operationsByPortfolio.ContainsKey(portfolioId) || !operationsByPortfolio[portfolioId].Any())
                {
                    logger.LogInformation("No hay operaciones contables de tipo {OperationType} para el PortfolioId: {PortfolioId}", operationTypeName, portfolioId);
                    errors.Add(AccountingInconsistency.Create(portfolioId, OperationTypeNames.Operation, $"No se encontraron operaciones contables de tipo {operationTypeName} para el portfolio", string.Empty));
                }
            }

            if (errors.Any())
            {
                await inconsistencyHandler.HandleInconsistenciesAsync(errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                return Result.Failure<bool>(Error.Problem("Accounting.Operations", "Se encontraron inconsistencias"));
            }

            var collectionAccount = operationsResult.Value.GroupBy(x => x.CollectionAccount).Select(x => x.Key).ToList();
            var treasury = await sender.Send(new GetAccountingOperationsTreasuriesQuery(command.PortfolioIds, collectionAccount), cancellationToken);

            if (!treasury.IsSuccess)
                return Result.Failure<bool>(Error.Validation("Error al obtener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));

            var treasuryByPortfolioId = treasury.Value.ToDictionary(x => x.PortfolioId, x => x);

            var accountingAssistants = await validator.ProcessOperationsInParallel(
                operationsResult.Value,
                treasuryByPortfolioId,
                command,
                operationTypeName,
                cancellationToken);

            if (!accountingAssistants.IsSuccess)
            {
                logger.LogInformation("Insertar errores en Redis para operaciones de tipo {OperationType}", operationTypeName);
                await inconsistencyHandler.HandleInconsistenciesAsync(accountingAssistants.Errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                return Result.Failure<bool>(Error.Problem("Accounting.Operations", "Se encontraron inconsistencias"));
            }

            if (!accountingAssistants.SuccessItems.Any())
            {
                logger.LogInformation("No hay operaciones contables de tipo {OperationType} que procesar", operationTypeName);
                return Result.Failure<bool>(Error.Problem("Accounting.Operations", $"No hay operaciones contables de tipo {operationTypeName} que procesar"));
            }

            var accountingOperationsSave = await sender.Send(new AddAccountingEntitiesCommand(accountingAssistants.SuccessItems), cancellationToken);

            if (accountingOperationsSave.IsFailure)
            {
                logger.LogWarning("No se pudieron guardar las operaciones contables de tipo {OperationType}: {Error}", operationTypeName, accountingOperationsSave.Error);
                return Result.Failure<bool>(Error.Problem("Accounting.Operations", $"No se pudieron guardar las operaciones contables de tipo {operationTypeName}"));
            }

            logger.LogInformation("Operaciones contables de tipo {OperationType} procesadas exitosamente", operationTypeName);
            return Result.Success(true);
        }
    }
}
