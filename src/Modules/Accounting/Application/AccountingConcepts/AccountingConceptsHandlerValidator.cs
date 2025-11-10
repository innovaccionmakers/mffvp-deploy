using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
using Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Products.IntegrationEvents.Portfolio.GetPortfolioInformation;
using Treasury.Domain.TreasuryMovements;

namespace Accounting.Application.AccountingConcepts
{
    public record class AccountingConceptsHandlerValidator(
        IRpcClient rpcClient,
        IOperationLocator operationLocator,
        ILogger<AccountingConceptsHandlerValidator> logger
        )

    {
        private readonly string IncomeNature = "Ingreso";
        private readonly string NoAccountingParameterizationMessage = "No existe parametrización contable";
        private readonly string Debit = AccountingActivity.Debit;
        private readonly string Credit = AccountingActivity.Credit;

        public async Task<ProcessingResult<AccountingAssistant, AccountingInconsistency>> AccountingConceptsValidator(
            AccountingConceptsCommand command,
            IReadOnlyCollection<TreasuryMovement> movements,
            IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse> treasuryByPortfolioId,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> conceptByPortfolioId,
            CancellationToken cancellationToken)
        {
            var accountingAssistants = new List<AccountingAssistant>();
            var errors = new List<AccountingInconsistency>();

            foreach (var movement in movements)
            {
                var accountTreasuries = treasuryByPortfolioId?
                    .Where(x => x.PortfolioId == movement.PortfolioId && x.AccountNumbers == movement?.BankAccount?.AccountNumber)
                    .FirstOrDefault();
                var accountConcepts = conceptByPortfolioId?
                    .Where(x => x.PortfolioId == movement.PortfolioId && x.Name == movement.TreasuryConcept?.Concept)
                    .FirstOrDefault();

                var bankAccount = movement.BankAccount?.ToString() ?? string.Empty;
                var counterpartyInfo = await GetCounterpartyInfoAsync(movement, cancellationToken);
                var natureValue = EnumHelper.GetEnumMemberValue(movement.TreasuryConcept?.Nature);
                var concept = movement.TreasuryConcept?.Concept ?? string.Empty;
                bool affectsAccount = movement.TreasuryConcept?.RequiresBankAccount == true;

                if (!ValidateAccountingAccounts(movement, concept, accountTreasuries, accountConcepts, natureValue, affectsAccount, out var accountValidationErrors))
                    errors.AddRange(accountValidationErrors);

                var accounts = DetermineAccountingAccounts(movement, accountTreasuries, accountConcepts, natureValue, affectsAccount);

                var accountingAssistant = AccountingAssistant.Create(
                    movement.PortfolioId,
                    counterpartyInfo.identification,
                    counterpartyInfo.verificationDigit,
                    counterpartyInfo.name,
                    command.ProcessDate.ToString("yyyyMM"),
                    command.ProcessDate,
                    $"Concepto de {natureValue}",
                    movement.Value,
                    natureValue
                );

                if (accountingAssistant.IsFailure)
                {
                    logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                        movement.PortfolioId, accountingAssistant.Error);
                    errors.Add(AccountingInconsistency.Create(movement.PortfolioId, $"{OperationTypeNames.Concepts} - {concept}", accountingAssistant.Error.Description));
                    continue;
                }

                accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(accounts.debitAccount, accounts.creditAccount));
            }

            return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
        }

        private async Task<(string identification, int verificationDigit, string name)> GetCounterpartyInfoAsync(TreasuryMovement movement, CancellationToken cancellationToken)
        {
            if (movement.CounterpartyId == null || movement.CounterpartyId == 0)
            {
                var portfolio = await rpcClient.CallAsync<GetPortfolioInformationByIdRequest, GetPortfolioInformationByIdResponse>(
                    new GetPortfolioInformationByIdRequest(movement.PortfolioId), cancellationToken);

                return (
                    portfolio.PortfolioInformation?.NitApprovedPortfolio ?? string.Empty,
                    portfolio.PortfolioInformation?.VerificationDigit ?? 0,
                    portfolio.PortfolioInformation?.Name ?? string.Empty
                );
            }
            else
            {
                return (
                    movement.Counterparty.Nit,
                    movement.Counterparty?.Digit ?? 0,
                    movement.Counterparty?.Description ?? string.Empty
                );
            }
        }
        private bool ValidateAccountingAccounts(
            TreasuryMovement movement,
            string concept,
            GetAccountingConceptsTreasuriesResponse accountTreasuries,
            GetConceptsByPortfolioIdsResponse accountConcepts,
            string natureValue,
            bool affectsAccount,
            out List<AccountingInconsistency> validationErrors)
        {
            validationErrors = new List<AccountingInconsistency>();

            try
            {
                return affectsAccount
                    ? ValidateWithAccountAffectation(movement, concept, accountTreasuries, accountConcepts, natureValue, validationErrors)
                    : ValidateWithoutAccountAffectation(movement, concept, accountConcepts, natureValue, validationErrors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al validar cuentas contables para el portafolio {PortfolioId}", movement.PortfolioId);
                validationErrors.Add(AccountingInconsistency.Create(movement.PortfolioId, $"{OperationTypeNames.Concepts} - {concept}", "Error al validar cuentas contables"));
                return false;
            }
        }

        private bool ValidateWithAccountAffectation(
            TreasuryMovement movement,
            string concept,
            GetAccountingConceptsTreasuriesResponse accountTreasuries,
            GetConceptsByPortfolioIdsResponse accountConcepts,
            string natureValue,
            List<AccountingInconsistency> validationErrors) =>
            natureValue == IncomeNature
                ? ValidateAccountConceptsCredit(movement, concept, accountConcepts, validationErrors) &
                  ValidateAccountTreasuriesDebit(movement, concept, accountTreasuries, validationErrors)
                : ValidateAccountConceptsDebit(movement, concept, accountConcepts, validationErrors) &
                  ValidateAccountTreasuriesCredit(movement, concept, accountTreasuries, validationErrors);

        private bool ValidateWithoutAccountAffectation(
            TreasuryMovement movement,
            string concept,
            GetConceptsByPortfolioIdsResponse accountConcepts,
            string natureValue,
            List<AccountingInconsistency> validationErrors) =>
            ValidateAccountConceptsDebit(movement, concept, accountConcepts, validationErrors) &
            ValidateAccountConceptsCredit(movement, concept, accountConcepts, validationErrors);

        private bool ValidateAccountConceptsCredit(TreasuryMovement movement, string concept,
            GetConceptsByPortfolioIdsResponse accountConcept, List<AccountingInconsistency> validationErrors) =>
            ValidateConceptAccount(accountConcept?.CreditAccount, movement, concept, Credit, validationErrors);

        private bool ValidateAccountConceptsDebit(TreasuryMovement movement, string concept,
            GetConceptsByPortfolioIdsResponse accountConcept, List<AccountingInconsistency> validationErrors) =>
            ValidateConceptAccount(accountConcept?.DebitAccount, movement, concept, Debit, validationErrors);

        private bool ValidateAccountTreasuriesDebit(TreasuryMovement movement, string concept,
            GetAccountingConceptsTreasuriesResponse accountTreasury, List<AccountingInconsistency> validationErrors) =>
            ValidateTreasuryAccount(accountTreasury?.DebitAccount, movement, concept, Debit, validationErrors);

        private bool ValidateAccountTreasuriesCredit(TreasuryMovement movement, string concept,
            GetAccountingConceptsTreasuriesResponse accountTreasury, List<AccountingInconsistency> validationErrors) =>
            ValidateTreasuryAccount(accountTreasury?.CreditAccount, movement, concept, Credit, validationErrors);

        private bool ValidateConceptAccount(string account, TreasuryMovement movement, string concept,
            string activity, List<AccountingInconsistency> validationErrors)
        {
            if (!string.IsNullOrWhiteSpace(account)) return true;

            logger.LogWarning("No se encontró cuenta de {Activity} para el portafolio {PortfolioId}",
                activity == AccountingActivity.Debit ? "débito" : "crédito", movement.PortfolioId);

            validationErrors.Add(AccountingInconsistency.Create(
                movement.PortfolioId,
                $"{OperationTypeNames.Concepts} - {concept}",
                NoAccountingParameterizationMessage,
                activity
            ));

            return false;
        }

        private bool ValidateTreasuryAccount(string account, TreasuryMovement movement, string concept,
            string activity, List<AccountingInconsistency> validationErrors)
        {
            if (!string.IsNullOrWhiteSpace(account)) return true;

            var accountType = activity == AccountingActivity.Debit ? "débito" : "crédito";
            logger.LogWarning("No se encontró cuenta de {AccountType} para el portafolio {PortfolioId}",
                accountType, movement.PortfolioId);

            var message = activity == AccountingActivity.Debit
                ? $"{NoAccountingParameterizationMessage} para la cuenta bancaria {movement.BankAccount?.AccountNumber}"
                : $"{NoAccountingParameterizationMessage} para la cuenta bancaria {movement.BankAccount?.AccountNumber}";

            validationErrors.Add(AccountingInconsistency.Create(
                movement.PortfolioId,
                $"{OperationTypeNames.Concepts} - {concept}",
                message,
                activity
            ));

            return false;
        }

        private (string debitAccount, string creditAccount) DetermineAccountingAccounts(
            TreasuryMovement movement,
            GetAccountingConceptsTreasuriesResponse accountTreasuries,
            GetConceptsByPortfolioIdsResponse accountConcepts,
            string natureValue,
            bool affectsAccount)
        {
            try
            {
                string debitAccount = string.Empty;
                string creditAccount = string.Empty;

                if (!affectsAccount)
                {
                    debitAccount = accountConcepts?.DebitAccount ?? string.Empty;
                    creditAccount = accountConcepts?.CreditAccount ?? string.Empty;
                }
                else
                {
                    if (natureValue == IncomeNature)
                    {
                        debitAccount = accountTreasuries?.DebitAccount ?? string.Empty;
                        creditAccount = accountConcepts?.CreditAccount ?? string.Empty;
                    }
                    else
                    {
                        debitAccount = accountConcepts?.DebitAccount ?? string.Empty;
                        creditAccount = accountTreasuries?.CreditAccount ?? string.Empty;
                    }
                }

                // Validar que las cuentas no sean nulas o vacías
                if (string.IsNullOrWhiteSpace(debitAccount) || string.IsNullOrWhiteSpace(creditAccount))
                {
                    logger.LogError("Cuentas contables inválidas para el portafolio {PortfolioId}. Débito: {DebitAccount}, Crédito: {CreditAccount}",
                        movement.PortfolioId, debitAccount ?? "null", creditAccount ?? "null");
                }

                return (debitAccount ?? string.Empty, creditAccount ?? string.Empty);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al determinar cuentas contables para el movimiento {MovementId}", movement.Id);
                return (string.Empty, string.Empty);
            }
        }
    }
}