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
using DocumentFormat.OpenXml.Office2010.Excel;
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
        ITreasuryLocator treasuryLocator,
        IPortfolioLocator portfolioLocator,
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
            var operationsResult = await operationLocator.GetAccountingOperationsAsync(
                command.PortfolioIds,
                command.ProcessDate,
                cancellationToken);

            if (!operationsResult.IsSuccess)
                return Result.Failure<bool>(Error.Validation(operationsResult.Error.Code ?? string.Empty, operationsResult.Error.Description ?? string.Empty));

            if (operationsResult.Value.Count == 0)
            {
                logger.LogInformation("No hay operaciones de tipo {OperationType} para procesar", OperationTypeAttributes.Names.Contribution);
                return Result.Success(true);
            }

            var clientOperationIds = operationsResult.Value.Select(op => op.ClientOperationId).Distinct().ToList();
            var collectionBankIdsResult = await operationLocator.GetCollectionBankIdsByClientOperationIdsAsync(clientOperationIds, cancellationToken);

            if (collectionBankIdsResult.IsFailure)
            {
                logger.LogWarning("Error al obtener los banco_recaudo para las operaciones: {Error}", collectionBankIdsResult.Error);
                return Result.Failure<bool>(Error.Validation(collectionBankIdsResult.Error.Code ?? string.Empty, collectionBankIdsResult.Error.Description ?? string.Empty));
            }

            var collectionBankIdsByClientOperationId = collectionBankIdsResult.Value;

            return await ProcessOperationsByTypeAsync(
                operationsResult.Value,
                command,
                OperationTypeAttributes.Names.Contribution,
                collectionBankIdsByClientOperationId,
                cancellationToken);
        }

        private async Task<Result<bool>> ProcessDebitNoteOperationsAsync(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            var operationsResult = await operationLocator.GetAccountingDebitNoteOperationsAsync(
                command.PortfolioIds,
                command.ProcessDate,
                cancellationToken);

            if (!operationsResult.IsSuccess)
                return Result.Failure<bool>(Error.Validation(operationsResult.Error.Code ?? string.Empty, operationsResult.Error.Description ?? string.Empty));

            if (operationsResult.Value.Count == 0)
            {
                logger.LogInformation("No hay operaciones de tipo {OperationType} para procesar", OperationTypeAttributes.Names.DebitNote);
                return Result.Success(true);
            }

            var linkedClientOperationIds = operationsResult.Value
                .Select(op => op.LinkedClientOperationId)
                .Where(id => id != null)
                .Distinct()
                .Cast<long>()
                .ToList();

            var collectionBankIdsResult = await operationLocator.GetCollectionBankIdsByClientOperationIdsAsync(linkedClientOperationIds, cancellationToken);

            if (collectionBankIdsResult.IsFailure)
            {
                logger.LogWarning("Error al obtener los banco_recaudo para las operaciones: {Error}", collectionBankIdsResult.Error);
                return Result.Failure<bool>(Error.Validation(collectionBankIdsResult.Error.Code ?? string.Empty, collectionBankIdsResult.Error.Description ?? string.Empty));
            }

            var collectionBankIdsByClientOperationId = collectionBankIdsResult.Value;

            return await ProcessOperationsByTypeAsync(
                operationsResult.Value,
                command,
                OperationTypeAttributes.Names.DebitNote,
                collectionBankIdsByClientOperationId,
                cancellationToken);
        }

        private async Task<Result<bool>> ProcessOperationsByTypeAsync(
            IReadOnlyCollection<GetAccountingOperationsResponse> operations,
            AccountingOperationsCommand command,
            string operationTypeName,
            Dictionary<long, int> collectionBankIdsByClientOperationId,
            CancellationToken cancellationToken)
        {
            var uniqueBankIds = collectionBankIdsByClientOperationId.Values.Distinct().Select(id => (long)id).ToList();
            Dictionary<long, IssuerInfo>? issuersByBankId = null;

            if (uniqueBankIds.Count == 0)
            {
                logger.LogInformation("No hay bancos de recaudo para obtener emisores");
                return Result.Failure<bool>(Error.Validation("Accounting.Operations", "No hay bancos de recaudo para obtener emisores"));
            }

            var issuersResult = await treasuryLocator.GetIssuersByIdsAsync(uniqueBankIds, cancellationToken);

            if (issuersResult.IsFailure)
            {
                logger.LogWarning("Error al obtener los emisores: {Error}", issuersResult.Error?.Description ?? "Error desconocido");
                return Result.Failure<bool>(Error.Validation(issuersResult.Error?.Code ?? string.Empty, issuersResult.Error?.Description ?? string.Empty));
            }

            issuersByBankId = issuersResult.Value.ToDictionary(issuer => issuer.Id, issuer => issuer);

            // Obtener información básica de portafolios
            var uniquePortfolioIds = operations.Select(op => op.PortfolioId).Distinct().ToList();
            var portfoliosResult = await portfolioLocator.GetPortfoliosBasicInformationByIdsAsync(uniquePortfolioIds, cancellationToken);

            if (portfoliosResult.IsFailure)
            {
                logger.LogWarning("Error al obtener la información de los portafolios: {Error}", portfoliosResult.Error?.Description ?? "Error desconocido");
                return Result.Failure<bool>(Error.Validation(portfoliosResult.Error?.Code ?? string.Empty, portfoliosResult.Error?.Description ?? string.Empty));
            }

            var errors = new ConcurrentBag<AccountingInconsistency>();
            var operationsByPortfolio = operations.GroupBy(op => op.PortfolioId).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var portfolioId in command.PortfolioIds)
            {
                if (!operationsByPortfolio.ContainsKey(portfolioId) || !operationsByPortfolio[portfolioId].Any())
                {
                    logger.LogInformation("No hay operaciones contables de tipo {OperationType} para el PortfolioId: {PortfolioId}", operationTypeName, portfolioId);
                    var transactionType = operationTypeName == OperationTypeAttributes.Names.DebitNote
                        ? OperationTypeNames.DebitNote
                        : OperationTypeNames.Operation;
                    errors.Add(AccountingInconsistency.Create(portfolioId, transactionType, $"No se encontraron operaciones contables de tipo {operationTypeName} para el portfolio", string.Empty));
                }
            }

            if (errors.Any())
            {
                await inconsistencyHandler.HandleInconsistenciesAsync(errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                return Result.Failure<bool>(Error.Problem("Accounting.Operations", "Se encontraron inconsistencias"));
            }

            var collectionAccount = operations.GroupBy(x => x.CollectionAccount).Select(x => x.Key).ToList();
            var treasury = await sender.Send(new GetAccountingOperationsTreasuriesQuery(command.PortfolioIds, collectionAccount), cancellationToken);

            if (!treasury.IsSuccess)
                return Result.Failure<bool>(Error.Validation("Error al obtener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));

            var treasuryByPortfolioId = treasury.Value.ToDictionary(x => x.PortfolioId, x => x);

            var accountingAssistants = await validator.ProcessOperationsInParallel(
                operations,
                treasuryByPortfolioId,
                command,
                operationTypeName,
                issuersByBankId,
                collectionBankIdsByClientOperationId,
                portfoliosResult.Value,
                cancellationToken);

            if (!accountingAssistants.IsSuccess)
            {
                logger.LogInformation("Insertar errores en Redis para operaciones de tipo {OperationType}", operationTypeName);
                await inconsistencyHandler.HandleInconsistenciesAsync(accountingAssistants.Errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                return Result.Failure<bool>(Error.Problem("Accounting.Operations", "Se encontraron inconsistencias"));
            }

            if (!accountingAssistants.SuccessItems.Any())
            {
                logger.LogInformation("No hay operaciones contables que procesar");
                return Result.Success(true);
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
