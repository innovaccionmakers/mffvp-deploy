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
                    .Where(x => x.PortfolioId == movement.PortfolioId && x.AccountNumbers == movement.BankAccount.AccountNumber)
                    .ToList() ?? new List<GetAccountingConceptsTreasuriesResponse>();

                var accountConcepts = conceptByPortfolioId?
                    .Where(x => x.PortfolioId == movement.PortfolioId && x.Name == movement.TreasuryConcept?.Concept)
                    .ToList() ?? new List<GetConceptsByPortfolioIdsResponse>();

                // 1. Obtener información del contraparte
                var bankAccount = movement.BankAccount?.ToString() ?? string.Empty;
                var counterpartyInfo = await GetCounterpartyInfoAsync(movement, cancellationToken);
                var identification = counterpartyInfo.identification;
                var verificationDigit = counterpartyInfo.verificationDigit;
                var name = counterpartyInfo.name;
                var natureValue = EnumHelper.GetEnumMemberValue(movement.TreasuryConcept?.Nature);
                var concept = movement.TreasuryConcept?.Concept ?? string.Empty;
                var detail = $"Concepto de {natureValue}";
                bool affectsAccount = movement.TreasuryConcept?.RequiresBankAccount == true;

                // 2. Validar cuentas contables para todas las cuentas del portafolio
                if (!ValidateAccountingAccounts(movement, concept, accountTreasuries, accountConcepts, natureValue, affectsAccount, out var accountValidationErrors))
                    errors.AddRange(accountValidationErrors);

                // 3. Determinar cuentas débito/crédito para todas las cuentas
                var accounts = DetermineAccountingAccounts(movement, accountTreasuries, accountConcepts);

                var accountingAssistant = AccountingAssistant.Create(
                    movement.PortfolioId,
                    identification,
                    verificationDigit,
                    name,
                    command.ProcessDate.ToString("yyyyMM"),
                    command.ProcessDate,
                    detail,
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
                    portfolio.PortfolioInformation?.PortfolioNIT ?? string.Empty,
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
            IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse> accountTreasuries,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> accountConcepts,
            string natureValue,
            bool affectsAccount,
            out List<AccountingInconsistency> validationErrors)
        {
            try
            {
                validationErrors = new List<AccountingInconsistency>();
                string message = "No existe parametrización contable";

                if (!affectsAccount)
                    return ValidateWithoutAccountAffectation(movement, concept, accountConcepts, natureValue, message, validationErrors);

                return ValidateWithAccountAffectation(movement, concept, accountTreasuries, accountConcepts, natureValue, message, validationErrors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al validar cuentas contables para el portafolio {PortfolioId}", movement.PortfolioId);
                validationErrors = new List<AccountingInconsistency>();
                return false;
            }
        }

        private bool ValidateWithAccountAffectation(
            TreasuryMovement movement,
            string concept,
            IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse> accountTreasuries,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> accountConcepts,
            string natureValue,
            string message,
            List<AccountingInconsistency> validationErrors)
        {
            bool isValid = true;

            if (natureValue == "Ingreso")
            {
                isValid &= ValidateAccountConceptsCredit(movement, concept, accountConcepts, message, validationErrors);
                isValid &= ValidateAccountTreasuriesDebit(movement, concept, accountTreasuries, message, validationErrors);
            }
            else
            {
                isValid &= ValidateAccountConceptsDebit(movement, concept, accountConcepts, message, validationErrors);
                isValid &= ValidateAccountTreasuriesCredit(movement, concept, accountTreasuries, message, validationErrors);
            }

            return isValid;
        }

        private bool ValidateWithoutAccountAffectation(
            TreasuryMovement movement,
            string concept,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> accountConcepts,
            string natureValue,
            string message,
            List<AccountingInconsistency> validationErrors)
        {
            bool isValid = true;

            if (natureValue == "Ingreso")
            {
                isValid &= ValidateAccountConceptsCredit(movement, concept, accountConcepts, message, validationErrors);
                isValid &= ValidateAccountConceptsDebit(movement, concept, accountConcepts, message, validationErrors);
            }
            else
            {
                isValid &= ValidateAccountConceptsDebit(movement, concept, accountConcepts, message, validationErrors);
                isValid &= ValidateAccountConceptsCredit(movement, concept, accountConcepts, message, validationErrors);
            }

            return isValid;
        }

        private bool ValidateAccountConceptsCredit(
            TreasuryMovement movement,
            string concept,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> accountConcepts,
            string message,
            List<AccountingInconsistency> validationErrors)
        {
            bool isValid = true;

            foreach (var accountConcept in accountConcepts)
            {
                if (string.IsNullOrWhiteSpace(accountConcept.CreditAccount))
                {
                    logger.LogWarning("No se encontró cuenta de crédito para el portafolio {PortfolioId}", movement.PortfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(movement.PortfolioId, $"{OperationTypeNames.Concepts} - {concept}", message, AccountingActivity.Credit));
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool ValidateAccountConceptsDebit(
            TreasuryMovement movement,
            string concept,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> accountConcepts,
            string message,
            List<AccountingInconsistency> validationErrors)
        {
            bool isValid = true;

            foreach (var accountConcept in accountConcepts)
            {
                if (string.IsNullOrWhiteSpace(accountConcept.DebitAccount))
                {
                    logger.LogWarning("No se encontró cuenta de débito para el portafolio {PortfolioId}", movement.PortfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(movement.PortfolioId, $"{OperationTypeNames.Concepts} - {concept}", message, AccountingActivity.Debit));
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool ValidateAccountTreasuriesDebit(
            TreasuryMovement movement,
            string concept,
            IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse> accountTreasuries,
            string message,
            List<AccountingInconsistency> validationErrors)
        {
            bool isValid = true;

            foreach (var accountTreasury in accountTreasuries)
            {
                if (string.IsNullOrWhiteSpace(accountTreasury.DebitAccount))
                {
                    logger.LogWarning("No se encontró cuenta de débito para el portafolio {PortfolioId}", movement.PortfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(movement.PortfolioId, $"{OperationTypeNames.Concepts} - {concept}", $"{message} para la cuenta bancaria {movement.BankAccount.AccountNumber}", AccountingActivity.Debit));
                    isValid = false;
                }
            }

            return isValid;
        }

        private bool ValidateAccountTreasuriesCredit(
            TreasuryMovement movement,
            string concept,
            IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse> accountTreasuries,
            string message,
            List<AccountingInconsistency> validationErrors)
        {
            bool isValid = true;

            foreach (var accountTreasury in accountTreasuries)
            {
                if (string.IsNullOrWhiteSpace(accountTreasury.CreditAccount))
                {
                    logger.LogWarning("No se encontró cuenta de débito de treasury para el portafolio {PortfolioId}", movement.PortfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(movement.PortfolioId, $"{OperationTypeNames.Concepts} - {concept}", $"{message} para la cuenta bancaria {movement.BankAccount.AccountNumber}", AccountingActivity.Credit));
                    isValid = false;
                }
            }

            return isValid;
        }

        private (string debitAccount, string creditAccount) DetermineAccountingAccounts(
            TreasuryMovement movement,
            IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse> accountTreasuries,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> accountConcepts)
        {
            try
            {
                bool isTreasuryConceptZero = movement.TreasuryConceptId == 0;
                bool requiresBankAccount = movement.TreasuryConcept?.RequiresBankAccount == true;

                string debitAccount = string.Empty;
                string creditAccount = string.Empty;

                // Obtener la primera cuenta válida de cada lista
                var validTreasuryAccount = accountTreasuries?.FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x.DebitAccount) && !string.IsNullOrWhiteSpace(x.CreditAccount));

                var validConceptAccount = accountConcepts?.FirstOrDefault(x =>
                    !string.IsNullOrWhiteSpace(x.DebitAccount) && !string.IsNullOrWhiteSpace(x.CreditAccount));

                if (isTreasuryConceptZero)
                {
                    debitAccount = requiresBankAccount ?
                        validTreasuryAccount?.DebitAccount ?? string.Empty :
                        validConceptAccount?.DebitAccount ?? string.Empty;

                    creditAccount = validConceptAccount?.CreditAccount ?? string.Empty;
                }
                else
                {
                    debitAccount = validConceptAccount?.DebitAccount ?? string.Empty;
                    creditAccount = requiresBankAccount ?
                        validTreasuryAccount?.CreditAccount ?? string.Empty :
                        validConceptAccount?.CreditAccount ?? string.Empty;
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