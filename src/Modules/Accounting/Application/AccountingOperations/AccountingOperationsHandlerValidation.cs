using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Domain;
using Customers.Integrations.People.GetPeopleByIdentifications;
using Microsoft.Extensions.Logging;
using Operations.Integrations.ClientOperations.GetAccountingOperations;
using System.Collections.Concurrent;

namespace Accounting.Application.AccountingOperations
{
    public record class AccountingOperationsHandlerValidation(
    IOperationLocator operationLocator,
    ILogger<AccountingOperationsHandlerValidation> logger)
    {
        public async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> ProcessOperationsInParallel(
           IReadOnlyCollection<GetAccountingOperationsResponse> operations,
           Dictionary<int, string> identificationByActivateId,
           Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
           Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
           Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
           DateTime processDate,
           CancellationToken cancellationToken)
        {
            var operationsList = operations.ToList();

            var batchSize = Math.Min(1000, operationsList.Count / (Environment.ProcessorCount * 2));
            batchSize = Math.Max(100, batchSize);

            var batches = operationsList.Chunk(batchSize);
            var allAccountingAssistants = new ConcurrentBag<AccountingAssistant>();
            var errors = new ConcurrentBag<AccountingInconsistency>();

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(batches, parallelOptions, async (batch, ct) =>
            {
                var batchResult = await ProcessBatchAsync(
                    batch,
                    identificationByActivateId,
                    peopleByIdentification,
                    treasuryByPortfolioId,
                    passiveTransactionByPortfolioId,
                    processDate,
                    ct);

                foreach (var assistant in batchResult.Assistants)
                {
                    allAccountingAssistants.Add(assistant);
                }

                foreach (var error in batchResult.Errors)
                {
                    errors.Add(error);
                }
            });

            return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(
                allAccountingAssistants.ToList(), errors.ToList());
        }

        public async Task<(List<AccountingAssistant> Assistants, List<AccountingInconsistency> Errors)> ProcessBatchAsync(
            GetAccountingOperationsResponse[] operations,
            Dictionary<int, string> identificationByActivateId,
            Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
            DateTime processDate,
            CancellationToken cancellationToken)
        {
            var results = new ConcurrentBag<(AccountingAssistant? Assistant, AccountingInconsistency? Error)>();

            await Parallel.ForEachAsync(operations, cancellationToken, async (operation, ct) =>
            {
                try
                {
                    var result = await ProcessSingleOperationAsync(
                        operation,
                        identificationByActivateId,
                        peopleByIdentification,
                        treasuryByPortfolioId,
                        passiveTransactionByPortfolioId,
                        processDate,
                        ct);

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error procesando operación para portfolio {PortfolioId}", operation.PortfolioId);
                    results.Add((null, AccountingInconsistency.Create(
                        operation.PortfolioId,
                        OperationTypeNames.Operation,
                        $"Error procesando operación: {ex.Message}")));
                }
            });

            var assistants = new List<AccountingAssistant>();
            var errors = new List<AccountingInconsistency>();

            foreach (var result in results)
            {
                if (result.Assistant != null)
                    assistants.Add(result.Assistant);
                if (result.Error != null)
                    errors.Add(result.Error);
            }

            return (assistants, errors);
        }

        private async ValueTask<(AccountingAssistant? Assistant, AccountingInconsistency? Error)> ProcessSingleOperationAsync(
            GetAccountingOperationsResponse operation,
            Dictionary<int, string> identificationByActivateId,
            Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
            DateTime processDate,
            CancellationToken cancellationToken)
        {
            await Task.Yield(); // Permitir que el scheduler maneje mejor el paralelismo

            // Optimizar búsquedas en diccionarios
            identificationByActivateId.TryGetValue(operation.AffiliateId, out var identification);

            GetPeopleByIdentificationsResponse? person = null;
            if (!string.IsNullOrEmpty(identification))
            {
                peopleByIdentification.TryGetValue(identification, out person);
            }

            treasuryByPortfolioId.TryGetValue(operation.PortfolioId, out var debitAccount);
            passiveTransactionByPortfolioId.TryGetValue(operation.PortfolioId, out var creditAccount);

            var natureValue = EnumHelper.GetEnumMemberValue(operation.Nature);

            // Validaciones optimizadas
            if (string.IsNullOrWhiteSpace(debitAccount?.DebitAccount))
            {
                return (null, AccountingInconsistency.Create(
                    operation.PortfolioId,
                    OperationTypeNames.Operation,
                    "No existe parametrización contable",
                    AccountingActivity.Debit));
            }

            if (string.IsNullOrWhiteSpace(creditAccount?.CreditAccount))
            {
                return (null, AccountingInconsistency.Create(
                    operation.PortfolioId,
                    OperationTypeNames.Operation,
                    "No existe parametrización contable",
                    AccountingActivity.Credit));
            }

            var accountingAssistant = AccountingAssistant.Create(
                operation.PortfolioId,
                identification ?? string.Empty,
                0,
                person?.FullName ?? string.Empty,
                processDate.ToString("yyyyMM"),
                processDate,
                operation.OperationTypeName,
                operation.Amount,
                natureValue
            );

            if (accountingAssistant.IsFailure)
            {
                return (null, AccountingInconsistency.Create(
                    operation.PortfolioId,
                    OperationTypeNames.Operation,
                    accountingAssistant.Error.Description));
            }

            var assistants = accountingAssistant.Value.ToDebitAndCredit(
                debitAccount?.DebitAccount,
                creditAccount?.CreditAccount);

            return (assistants.FirstOrDefault(), null);
        }
    }
}
