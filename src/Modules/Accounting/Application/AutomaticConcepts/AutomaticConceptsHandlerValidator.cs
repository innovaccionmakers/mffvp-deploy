using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AutomaticConcepts;
using Closing.IntegrationEvents.Yields;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Operations.IntegrationEvents.OperationTypes;

namespace Accounting.Application.AutomaticConcepts
{
    public record class AutomaticConceptsHandlerValidator(
        IPortfolioLocator portfolioLocator,
        IOperationLocator operationLocator,
        IPassiveTransactionRepository passiveTransactionRepository,
        ILogger<AutomaticConceptsHandlerValidator> logger)
    {
        public async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> AutomaticConceptsValidator(
            AutomaticConceptsCommand command,
            GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse yieldResult,
            GetOperationTypeByNameResponse operationsType,
            string automaticConcept,
            CancellationToken cancellationToken)
        {
            var accountingAssistants = new List<AccountingAssistant>();
            var errors = new List<AccountingInconsistency>();

            foreach (var yield in yieldResult.YieldAutConcepts.Yields)
            {
                if (yield.CreditedYields == yield.YieldToCredit)
                    continue;

                var value = yield.YieldToCredit - yield.CreditedYields;
                var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);
                IncomeEgressNature naturalezaFiltro = value < 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress;
                var operationType = operationsType.OperationType.FirstOrDefault(ot => ot.Name == automaticConcept && ot.Nature == naturalezaFiltro);
                var natureValue = operationLocator.GetEnumMemberValue(operationType!.Nature);
                var passiveTransaction = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(yield.PortfolioId, operationType.OperationTypeId, cancellationToken);

                var accountingAccounts = new AccountingAccounts(
                    yield.PortfolioId,
                    operationType.OperationTypeId,
                    passiveTransaction,
                    passiveTransaction?.CreditAccount,
                    passiveTransaction?.DebitAccount,
                    AccountingActivity.Credit,
                    AccountingActivity.Debit
                    );

                if (!ValidateAccountingAccounts(accountingAccounts, out var accountValidationErrors))
                    errors.AddRange(accountValidationErrors);

                var accountingAssistant = AccountingAssistant.Create(
                    yield.PortfolioId,
                    portfolioResult.Value.NitApprovedPortfolio,
                    portfolioResult.Value.VerificationDigit,
                    portfolioResult.Value.Name,
                    command.ProcessDate.ToString("yyyyMM"),
                    command.ProcessDate,
                    operationType.Name,
                    Math.Abs(value),
                    natureValue
                    );

                if (accountingAssistant.IsFailure)
                {

                    logger.LogError($"Error procesando los conceptos automáticos para portfolio {yield.PortfolioId}", yield.PortfolioId);
                    errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Operation, accountingAssistant.Error.Description));
                    continue;
                }

                accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(passiveTransaction?.DebitAccount, passiveTransaction?.CreditAccount));
            }

            foreach (var yield in yieldResult.YieldAutConcepts.YieldDetails)
            {
                if (yield.Income == 0)
                    continue;

                var value = yield.Income;
                var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(yield.PortfolioId, cancellationToken);
                IncomeEgressNature naturalezaFiltro = value > 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress;
                var operationType = operationsType.OperationType.FirstOrDefault(ot => ot.Name == automaticConcept && ot.Nature == naturalezaFiltro);
                var natureValue = operationLocator.GetEnumMemberValue(value < 0 ? IncomeEgressNature.Income : IncomeEgressNature.Egress);
                var passiveTransaction = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(yield.PortfolioId, operationType.OperationTypeId, cancellationToken);

                var accountingAccounts = new AccountingAccounts(
                    yield.PortfolioId,
                    operationType.OperationTypeId,
                    passiveTransaction,
                    passiveTransaction?.CreditAccount,
                    passiveTransaction?.DebitAccount,
                    AccountingActivity.ContraCreditAccount,
                    AccountingActivity.ContraDebitAccount
                    );

                if (!ValidateAccountingAccounts(accountingAccounts, out var accountValidationErrors))
                    errors.AddRange(accountValidationErrors);

                var accountingAssistant = AccountingAssistant.Create(
                    yield.PortfolioId,
                    portfolioResult.Value.NitApprovedPortfolio,
                    portfolioResult.Value.VerificationDigit,
                    portfolioResult.Value.Name,
                    command.ProcessDate.ToString("yyyyMM"),
                    command.ProcessDate,
                    operationType.Name,
                    Math.Abs(value),
                    natureValue
                    );

                if (accountingAssistant.IsFailure)
                {

                    logger.LogError($"Error procesando los conceptos automáticos para portfolio {yield.PortfolioId}", yield.PortfolioId);
                    errors.Add(AccountingInconsistency.Create(yield.PortfolioId, OperationTypeNames.Operation, accountingAssistant.Error.Description));
                    continue;
                }

                accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(passiveTransaction?.ContraDebitAccount, passiveTransaction?.ContraCreditAccount));
            }

            return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
        }

        private bool ValidateAccountingAccounts(AccountingAccounts accountingAccounts, out List<AccountingInconsistency> validationErrors)
        {
            validationErrors = new List<AccountingInconsistency>();
            bool isValid = true;


            if (accountingAccounts.passiveTransaction == null)
            {
                logger.LogWarning("No se encontraron conceptos automáticos para el portafolio {PortfolioId} y el tipo operación {OperationType}", accountingAccounts.portfolioId, accountingAccounts.operationTypeId);
                validationErrors.Add(AccountingInconsistency.Create(accountingAccounts.portfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", accountingAccounts.Credit));
                validationErrors.Add(AccountingInconsistency.Create(accountingAccounts.portfolioId, OperationTypeNames.AutomaticConcepts, "No existe parametrización contable", accountingAccounts.Debit));
                return false;
            }

            if (accountingAccounts.passiveTransactionCredit.IsNullOrEmpty())
            {
                logger.LogWarning("El concepto automático para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de crédito", accountingAccounts.portfolioId, accountingAccounts.operationTypeId);
                validationErrors.Add(AccountingInconsistency.Create(accountingAccounts.portfolioId, OperationTypeNames.AutomaticConcepts, "No existe cuenta de crédito", accountingAccounts.Credit));
                return false;
            }

            if (accountingAccounts.passiveTransactionDebit.IsNullOrEmpty())
            {
                logger.LogWarning("El concepto automático para el portafolio {PortfolioId} y el tipo operación {OperationType} no tiene cuenta de débito", accountingAccounts.portfolioId, accountingAccounts.operationTypeId);
                validationErrors.Add(AccountingInconsistency.Create(accountingAccounts.portfolioId, OperationTypeNames.AutomaticConcepts, "No existe cuenta de débito", accountingAccounts.Debit));
                return false;
            }

            return isValid;
        }


        private sealed record AccountingAccounts(
            int portfolioId,
            long operationTypeId,
            Domain.PassiveTransactions.PassiveTransaction? passiveTransaction,
            string? passiveTransactionCredit,
            string? passiveTransactionDebit,
            string Credit,
            string Debit
            );
    }
}
