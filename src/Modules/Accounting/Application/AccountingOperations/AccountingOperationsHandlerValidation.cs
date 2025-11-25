using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Associate.IntegrationsEvents.GetActivateIds;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using Customers.IntegrationEvents.PeopleByIdentificationsValidation;
using Customers.Integrations.People.GetPeopleByIdentifications;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.Integrations.ClientOperations.GetAccountingOperations;
using System.Collections.Concurrent;

namespace Accounting.Application.AccountingOperations
{
    public record class AccountingOperationsHandlerValidation(
        IOperationLocator operationLocator,
        IRpcClient rpcClient,
        ISender sender,
        ILogger<AccountingOperationsHandlerValidation> logger)
    {
        public async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> ProcessOperationsInParallel(
            IReadOnlyCollection<GetAccountingOperationsResponse> operations,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            AccountingOperationsCommand command,
            string operationTypeName,
            CancellationToken cancellationToken)
        {
            try
            {
                var operationsList = operations.ToList();

                //PassiveTransaction
                var operationTypeId = operations.GroupBy(x => x.OperationTypeId).Select(x => x.Key).ToList();
                var passiveTransaction = await sender.Send(new GetAccountingOperationsPassiveTransactionQuery(command.PortfolioIds, operationTypeId), cancellationToken);
                if (!passiveTransaction.IsSuccess)
                    return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(new List<AccountingAssistant>(), new List<AccountingInconsistency>());
                var passiveTransactionByPortfolioId = passiveTransaction.Value.ToDictionary(x => x.PortfolioId, x => x);

                if (operationsList.Count == 0)
                    return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(
                        new List<AccountingAssistant>(), new List<AccountingInconsistency>());

                var uniquePortfolioIds = operationsList.Select(op => op.PortfolioId).Distinct().ToList();
                var validPortfolios = new ConcurrentDictionary<int, bool>();
                var portfolioErrors = new ConcurrentBag<AccountingInconsistency>();

                await Parallel.ForEachAsync(uniquePortfolioIds, cancellationToken, async (portfolioId, ct) =>
                {
                    var validationResult = operationTypeName switch
                    {
                        OperationTypeAttributes.Names.Contribution => ValidateContributionPortfolio(
                            portfolioId,
                            treasuryByPortfolioId,
                            passiveTransactionByPortfolioId),
                        OperationTypeAttributes.Names.DebitNote => ValidateDebitNotePortfolio(
                            portfolioId,
                            treasuryByPortfolioId,
                            passiveTransactionByPortfolioId),
                        _ => ValidateContributionPortfolio(
                            portfolioId,
                            treasuryByPortfolioId,
                            passiveTransactionByPortfolioId)
                    };

                    if (validationResult.HasErrors)
                    {
                        foreach (var error in validationResult.Errors)
                        {
                            portfolioErrors.Add(error);
                        }
                    }

                    if (validationResult.IsValid)
                    {
                        validPortfolios.TryAdd(portfolioId, true);
                    }
                });

                if (portfolioErrors.Any())
                    return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(
                        new List<AccountingAssistant>(), portfolioErrors.ToList());

                var validOperations = operationsList
                    .Where(op => validPortfolios.ContainsKey(op.PortfolioId))
                    .ToList();

                if (validOperations.Count == 0)
                    return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(
                        new List<AccountingAssistant>(), new List<AccountingInconsistency>());

                var batchSize = Math.Min(1000, validOperations.Count / (Environment.ProcessorCount * 2));
                batchSize = Math.Max(100, batchSize);

                var allAccountingAssistants = new ConcurrentBag<AccountingAssistant>();
                var processingErrors = new ConcurrentBag<AccountingInconsistency>();

                var parallelOptions = new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };

                //Identifications
                var activateIds = operations.GroupBy(x => x.AffiliateId).Select(x => x.Key).ToList();
                var identificationResult = await rpcClient.CallAsync<GetIdentificationByActivateIdsRequestEvent, GetIdentificationByActivateIdsResponseEvent>(
                                                                new GetIdentificationByActivateIdsRequestEvent(activateIds), cancellationToken);
                if (!identificationResult.IsValid)
                    return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(new List<AccountingAssistant>(), new List<AccountingInconsistency>());
                var identificationByActivateId = identificationResult.Indentifications.ToDictionary(x => x.ActivateIds, x => x.Identification);

                //People
                var identifications = identificationResult.Indentifications.GroupBy(x => x.Identification).Select(x => x.Key).ToList();
                var people = await rpcClient.CallAsync<GetPersonByIdentificationsRequestEvent, GetPeopleByIdentificationsResponseEvent>(
                                                                new GetPersonByIdentificationsRequestEvent(identifications), cancellationToken);
                if (!people.IsValid)
                    return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(new List<AccountingAssistant>(), new List<AccountingInconsistency>());
                var peopleByIdentification = people.Person?.ToDictionary(x => x.Identification, x => x) ?? new Dictionary<string, GetPeopleByIdentificationsResponse>();

                var batches = validOperations.Chunk(batchSize);

                await Parallel.ForEachAsync(batches, parallelOptions, async (batch, ct) =>
                {
                    var batchResult = await ProcessBatchAsync(
                        batch,
                        identificationByActivateId,
                        peopleByIdentification,
                        treasuryByPortfolioId,
                        passiveTransactionByPortfolioId,
                        command.ProcessDate,
                        operationTypeName,
                        ct);

                    foreach (var assistant in batchResult.Assistants)
                    {
                        allAccountingAssistants.Add(assistant);
                    }

                    foreach (var error in batchResult.Errors)
                    {
                        processingErrors.Add(error);
                    }
                });

                return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(
                    allAccountingAssistants.ToList(), processingErrors.ToList());
            }
            catch (Exception)
            {
                return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(new List<AccountingAssistant>(), new List<AccountingInconsistency>());
            }

        }

        public async Task<(List<AccountingAssistant> Assistants, List<AccountingInconsistency> Errors)> ProcessBatchAsync(
            GetAccountingOperationsResponse[] operations,
            Dictionary<int, string> identificationByActivateId,
            Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
            DateTime processDate,
            string operationTypeName,
            CancellationToken cancellationToken)
        {
            var results = new ConcurrentBag<(List<AccountingAssistant> Assistants, List<AccountingInconsistency> Errors)>();

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
                        operationTypeName,
                        ct);

                    results.Add((result.Assistants, result.Errors));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error procesando operación para portfolio {PortfolioId}", operation.PortfolioId);
                    var transactionType = operationTypeName == OperationTypeAttributes.Names.DebitNote
                        ? OperationTypeNames.DebitNote
                        : OperationTypeNames.Operation;
                    results.Add((new List<AccountingAssistant>(), new List<AccountingInconsistency>
            {
                AccountingInconsistency.Create(
                    operation.PortfolioId,
                    transactionType,
                    $"Error procesando operación: {ex.Message}")
            }));
                }
            });

            var assistants = new List<AccountingAssistant>();
            var errors = new List<AccountingInconsistency>();

            foreach (var result in results)
            {
                if (result.Assistants.Any())
                    assistants.AddRange(result.Assistants);
                if (result.Errors.Any())
                    errors.AddRange(result.Errors);
            }

            return (assistants, errors);
        }

        private async ValueTask<(List<AccountingAssistant> Assistants, List<AccountingInconsistency> Errors)> ProcessSingleOperationAsync(
            GetAccountingOperationsResponse operation,
            Dictionary<int, string> identificationByActivateId,
            Dictionary<string, GetPeopleByIdentificationsResponse> peopleByIdentification,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId,
            DateTime processDate,
            string operationTypeName,
            CancellationToken cancellationToken)
        {
            await Task.Yield();

            string? debitAccountNumber;
            string? creditAccountNumber;


            if (operationTypeName == OperationTypeAttributes.Names.DebitNote)
            {
                debitAccountNumber = passiveTransactionByPortfolioId.ContainsKey(operation.PortfolioId)
                    ? passiveTransactionByPortfolioId[operation.PortfolioId]?.DebitAccount
                    : null;
                creditAccountNumber = treasuryByPortfolioId.ContainsKey(operation.PortfolioId)
                    ? treasuryByPortfolioId[operation.PortfolioId]?.CreditAccount
                    : null;
            }
            else
            {
                debitAccountNumber = treasuryByPortfolioId.ContainsKey(operation.PortfolioId)
                    ? treasuryByPortfolioId[operation.PortfolioId]?.DebitAccount
                    : null;
                creditAccountNumber = passiveTransactionByPortfolioId.ContainsKey(operation.PortfolioId)
                    ? passiveTransactionByPortfolioId[operation.PortfolioId]?.CreditAccount
                    : null;
            }

            identificationByActivateId.TryGetValue(operation.AffiliateId, out var identification);

            GetPeopleByIdentificationsResponse? person = null;
            if (!string.IsNullOrEmpty(identification))
            {
                peopleByIdentification.TryGetValue(identification, out person);
            }

            var natureValue = EnumHelper.GetEnumMemberValue(operation.Nature);

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
                var transactionType = operationTypeName == OperationTypeAttributes.Names.DebitNote
                    ? OperationTypeNames.DebitNote
                    : OperationTypeNames.Operation;
                return (new List<AccountingAssistant>(), new List<AccountingInconsistency>
                {
                    AccountingInconsistency.Create(
                        operation.PortfolioId,
                        transactionType,
                        accountingAssistant.Error.Description)
                });
            }

            var assistants = accountingAssistant.Value.ToDebitAndCredit(
                debitAccountNumber,
                creditAccountNumber).ToList();

            return (assistants, new List<AccountingInconsistency>());
        }

        private (bool IsValid, bool HasErrors, List<AccountingInconsistency> Errors) ValidateContributionPortfolio(
            int portfolioId,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId)
        {
            var errors = new List<AccountingInconsistency>();
            var hasDebit = treasuryByPortfolioId.ContainsKey(portfolioId) &&
                          !string.IsNullOrWhiteSpace(treasuryByPortfolioId[portfolioId]?.DebitAccount);
            var hasCredit = passiveTransactionByPortfolioId.ContainsKey(portfolioId) &&
                           !string.IsNullOrWhiteSpace(passiveTransactionByPortfolioId[portfolioId]?.CreditAccount);

            if (!hasDebit)
            {
                errors.Add(AccountingInconsistency.Create(
                    portfolioId, OperationTypeNames.Operation, "No existe parametrización contable", AccountingActivity.Debit));
            }

            if (!hasCredit)
            {
                errors.Add(AccountingInconsistency.Create(
                    portfolioId, OperationTypeNames.Operation, "No existe parametrización contable", AccountingActivity.Credit));
            }

            var isValid = hasDebit && hasCredit;
            return (isValid, errors.Any(), errors);
        }

        private (bool IsValid, bool HasErrors, List<AccountingInconsistency> Errors) ValidateDebitNotePortfolio(
            int portfolioId,
            Dictionary<int, GetAccountingOperationsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetAccountingOperationsPassiveTransactionResponse> passiveTransactionByPortfolioId)
        {
            var errors = new List<AccountingInconsistency>();

            var hasDebit = passiveTransactionByPortfolioId.ContainsKey(portfolioId) &&
                          !string.IsNullOrWhiteSpace(passiveTransactionByPortfolioId[portfolioId]?.DebitAccount);

            var hasCredit = treasuryByPortfolioId.ContainsKey(portfolioId) &&
                           !string.IsNullOrWhiteSpace(treasuryByPortfolioId[portfolioId]?.CreditAccount);

            if (!hasDebit)
            {
                errors.Add(AccountingInconsistency.Create(
                    portfolioId, OperationTypeNames.DebitNote, "No existe parametrización contable", AccountingActivity.Debit));
            }

            if (!hasCredit)
            {
                errors.Add(AccountingInconsistency.Create(
                    portfolioId, OperationTypeNames.DebitNote, "No existe parametrización contable", AccountingActivity.Credit));
            }

            var isValid = hasDebit && hasCredit;
            return (isValid, errors.Any(), errors);
        }
    }
}
