using Accounting.Domain.AccountingAssistants;
using Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Common.SharedKernel.Core.Primitives;
using Customers.Integrations.People.GetPeopleByIdentifications;
using Microsoft.Extensions.Logging;
using Operations.Integrations.ClientOperations.GetAccountingOperations;
using System.Collections.Concurrent;

namespace Accounting.Application.AccountingOperations
{
    public record class AccountingOperationsHandlerValidation(ILogger<AccountingOperationsHandlerValidation> logger)
    {
        public async Task<List<AccountingAssistant>> ProcessOperationsInParallel(
           IReadOnlyCollection<GetAccountingOperationsResponse> operations,
           Dictionary<int, string> identificationByActivateId,
           Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
           Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
           Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
           DateTime processDate,
           CancellationToken cancellationToken)
        {
            var operationsList = operations.ToList();
            var batchSize = 10000;
            var batches = operationsList.Chunk(batchSize);

            var allAccountingAssistants = new ConcurrentBag<AccountingAssistant>();
            var allErrors = new ConcurrentBag<Error>();

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };

            await Parallel.ForEachAsync(batches, parallelOptions, async (batch, ct) =>
            {
                var batchAssistants = ProcessBatch(
                    batch,
                    identificationByActivateId,
                    peopleByIdentification,
                    treasuryByPortfolioId,
                    passiveTransactionByPortfolioId,
                    processDate);

                foreach (var assistant in batchAssistants.Assistants)
                {
                    allAccountingAssistants.Add(assistant);
                }

                foreach (var error in batchAssistants.Errors)
                {
                    allErrors.Add(error);
                }
            });

            // Manejar errores acumulados
            if (allErrors.Count > 0)
            {
                logger.LogError($"Se encontraron {allErrors} errores durante el procesamiento", allErrors.Count);
                // Puedes decidir si continuar o fallar basado en tu lógica de negocio
            }

            return allAccountingAssistants.ToList();
        }

        public (List<AccountingAssistant> Assistants, List<Error> Errors) ProcessBatch(
            GetAccountingOperationsResponse[] operations,
            Dictionary<int, string> identificationByActivateId,
            Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
            DateTime processDate)
        {
            var assistants = new List<AccountingAssistant>();
            var errors = new List<Error>();

            foreach (var operation in operations)
            {
                try
                {
                    identificationByActivateId.TryGetValue(operation.AffiliateId, out var identification);
                    peopleByIdentification!.TryGetValue(identification ?? string.Empty, out var person);
                    treasuryByPortfolioId.TryGetValue(operation.PortfolioId, out var debitAccount);
                    passiveTransactionByPortfolioId.TryGetValue(operation.PortfolioId, out var creditAccount);

                    var accountingAssistant = AccountingAssistant.Create(
                        identification ?? string.Empty,
                        0,
                        person?.FullName ?? string.Empty,
                        processDate.ToString("yyyyMM"),
                        processDate,
                        operation.OperationTypeName,
                        operation.Amount,
                        operation.Nature.ToString()
                    );

                    if (accountingAssistant.IsFailure)
                    {
                        errors.Add(accountingAssistant.Error);
                        continue;
                    }

                    var debitCreditAssistants = accountingAssistant.Value.ToDebitAndCredit(debitAccount?.DebitAccount, creditAccount?.CreditAccount);

                    assistants.AddRange(debitCreditAssistants);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error procesando operación para portfolio {operation.PortfolioId}", operation.PortfolioId);
                    errors.Add(Error.Failure("ProcessingError", $"Error procesando operación: {ex.Message}"));
                }
            }

            return (assistants, errors);
        }
    }
}
