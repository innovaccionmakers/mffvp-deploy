using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Common.SharedKernel.Domain;
using Customers.Integrations.People.GetPeopleByIdentifications;
using Microsoft.Extensions.Logging;
using Operations.Integrations.ClientOperations.GetAccountingOperations;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;

namespace Accounting.Application.AccountingOperations
{
    public record class AccountingOperationsHandlerValidation(ILogger<AccountingOperationsHandlerValidation> logger)
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
            var batchSize = 10000;
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
                    errors.Add(error);
                }
            });

            return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(allAccountingAssistants.ToList(), errors);
        }

        public (List<AccountingAssistant> Assistants, List<AccountingInconsistency> Errors) ProcessBatch(
            GetAccountingOperationsResponse[] operations,
            Dictionary<int, string> identificationByActivateId,
            Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
            DateTime processDate)
        {
            var assistants = new List<AccountingAssistant>();
            var errors = new List<AccountingInconsistency>();

            foreach (var operation in operations)
            {
                try
                {
                    identificationByActivateId.TryGetValue(operation.AffiliateId, out var identification);
                    peopleByIdentification!.TryGetValue(identification ?? string.Empty, out var person);
                    treasuryByPortfolioId.TryGetValue(operation.PortfolioId, out var debitAccount);
                    passiveTransactionByPortfolioId.TryGetValue(operation.PortfolioId, out var creditAccount);
                    var natureValue = GetEnumMemberValue(operation.Nature);

                    if (string.IsNullOrWhiteSpace(debitAccount?.DebitAccount))
                    {
                        logger.LogWarning("No se encontró un concepto de cuenta de crédito para el portafolio {PortfolioId}", operation.PortfolioId);
                        errors.Add(AccountingInconsistency.Create(operation.PortfolioId, OperationTypeNames.Commission, "No existe parametrización contable", AccountingActivity.Debit));
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(creditAccount?.CreditAccount))
                    {
                        logger.LogWarning("No se encontró un concepto de cuenta de debito para el portafolio {PortfolioId}", operation.PortfolioId);
                        errors.Add(AccountingInconsistency.Create(operation.PortfolioId, OperationTypeNames.Commission, "No existe parametrización contable", AccountingActivity.Credit));
                        continue;
                    }

                    var accountingAssistant = AccountingAssistant.Create(
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

                        logger.LogError($"Error procesando operación para portfolio {operation.PortfolioId}", operation.PortfolioId);
                        errors.Add(AccountingInconsistency.Create(operation.PortfolioId, OperationTypeNames.Operation, accountingAssistant.Error.Description));
                        continue;
                    }

                    var debitCreditAssistants = accountingAssistant.Value.ToDebitAndCredit(debitAccount?.DebitAccount, creditAccount?.CreditAccount);

                    assistants.AddRange(debitCreditAssistants);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error procesando operación para portfolio {operation.PortfolioId}", operation.PortfolioId);
                    errors.Add(AccountingInconsistency.Create(operation.PortfolioId, OperationTypeNames.Operation, $"Error procesando operación para portfolio {operation.PortfolioId}"));
                }
            }

            return (assistants, errors);
        }

        public static string GetEnumMemberValue(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();
            return attribute?.Value ?? value.ToString();
        }
    }
}
