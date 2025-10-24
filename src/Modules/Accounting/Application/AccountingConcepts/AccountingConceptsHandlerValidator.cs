using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
using Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries;
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
            Dictionary<int, GetAccountingConceptsTreasuriesResponse> treasuryByPortfolioId,
            Dictionary<int, GetConceptsByPortfolioIdsResponse> conceptByPortfolioId,
            CancellationToken cancellationToken)
        {
            var accountingAssistants = new List<AccountingAssistant>();
            var errors = new List<AccountingInconsistency>();
            var identification = string.Empty;
            int verificationDigit = 0;
            string name = string.Empty;
            string debitAccount = string.Empty;
            string creditAccount = string.Empty;

            foreach (var movement in movements)
            {
                var accountTreasury = treasuryByPortfolioId.GetValueOrDefault(movement.PortfolioId);
                var accountConcept = conceptByPortfolioId.GetValueOrDefault(movement.PortfolioId);

                // 1. Obtener información del contraparte
                var counterpartyInfo = await GetCounterpartyInfoAsync(movement, cancellationToken);
                identification = counterpartyInfo.identification;
                verificationDigit = counterpartyInfo.verificationDigit;
                name = counterpartyInfo.name;
                var natureValue = operationLocator.GetEnumMemberValue(movement.TreasuryConcept?.Nature);

                // 2. Validar cuentas contables
                if (!ValidateAccountingAccounts(movement.PortfolioId, accountTreasury, accountConcept, out var accountValidationErrors))
                    errors.AddRange(accountValidationErrors);

                // 3. Determinar cuentas débito/crédito
                var accounts = DetermineAccountingAccounts(movement, accountTreasury, accountConcept);

                var accountingAssistant = AccountingAssistant.Create(
                    identification,
                    verificationDigit,
                    name,
                    command.ProcessDate.ToString("yyyyMM"),
                    command.ProcessDate,
                    movement.TreasuryConcept?.Observations ?? string.Empty,
                    movement.Value, 
                    natureValue
                );

                if (accountingAssistant.IsFailure)
                {
                    logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}",
                        movement.PortfolioId, accountingAssistant.Error);
                    errors.Add(AccountingInconsistency.Create(movement.PortfolioId, OperationTypeNames.Concepts, accountingAssistant.Error.Description));
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
            int portfolioId,
            GetAccountingConceptsTreasuriesResponse accountTreasury,
            GetConceptsByPortfolioIdsResponse accountConcept,
            out List<AccountingInconsistency> validationErrors)
        {
            try
            {
                validationErrors = new List<AccountingInconsistency>();
                bool isValid = true;

                // Validar cuentas del treasury
                if (string.IsNullOrWhiteSpace(accountTreasury?.DebitAccount))
                {
                    logger.LogWarning("No se encontró un concepto de cuenta de debito de treasury para el portafolio {PortfolioId}", portfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, OperationTypeNames.Concepts, "No existe parametrización contable", AccountingActivity.Debit));
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(accountTreasury?.CreditAccount))
                {
                    logger.LogWarning("No se encontró un concepto de cuenta de crédito de treasury para el portafolio {PortfolioId}", portfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, OperationTypeNames.Concepts, "No existe parametrización contable", AccountingActivity.Credit));
                    isValid = false;
                }

                // Validar cuentas del concepto
                if (string.IsNullOrWhiteSpace(accountConcept?.DebitAccount))
                {
                    logger.LogWarning("No se encontró un concepto de cuenta de crédito para el portafolio {PortfolioId}", portfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, OperationTypeNames.Concepts, "No existe parametrización contable", AccountingActivity.Debit));
                    isValid = false;
                }

                if (string.IsNullOrWhiteSpace(accountConcept?.CreditAccount))
                {
                    logger.LogWarning("No se encontró un concepto de cuenta de debito para el portafolio {PortfolioId}", portfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, OperationTypeNames.Concepts, "No existe parametrización contable", AccountingActivity.Credit));
                    isValid = false;
                }

                return isValid;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        private (string debitAccount, string creditAccount) DetermineAccountingAccounts(
            TreasuryMovement movement,
            GetAccountingConceptsTreasuriesResponse accountTreasury,
            GetConceptsByPortfolioIdsResponse accountConcept)
        {
            bool isTreasuryConceptZero = movement.TreasuryConceptId == 0;
            bool requiresBankAccount = movement.TreasuryConcept?.RequiresBankAccount == true;

            string debitAccount, creditAccount;

            if (isTreasuryConceptZero)
            {
                debitAccount = requiresBankAccount ? accountTreasury?.DebitAccount : accountConcept?.DebitAccount;
                creditAccount = accountConcept?.CreditAccount;
            }
            else
            {
                debitAccount = accountConcept?.DebitAccount;
                creditAccount = requiresBankAccount ? accountTreasury?.CreditAccount : accountConcept?.CreditAccount;
            }

            if (string.IsNullOrWhiteSpace(debitAccount) || string.IsNullOrWhiteSpace(creditAccount))
            {
                logger.LogError("Cuentas contables inválidas para el portafolio {PortfolioId}. Débito: {DebitAccount}, Crédito: {CreditAccount}",
                    movement.PortfolioId, debitAccount, creditAccount);
            }

            return (debitAccount, creditAccount);
        }
    }
}
