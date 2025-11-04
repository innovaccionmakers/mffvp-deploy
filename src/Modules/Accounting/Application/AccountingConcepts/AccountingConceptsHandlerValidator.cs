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
                    .Where(x => x.PortfolioId == movement.PortfolioId)
                    .ToList() ?? new List<GetAccountingConceptsTreasuriesResponse>();

                var accountConcepts = conceptByPortfolioId?
                    .Where(x => x.PortfolioId == movement.PortfolioId)
                    .ToList() ?? new List<GetConceptsByPortfolioIdsResponse>();

                // 1. Obtener información del contraparte
                var counterpartyInfo = await GetCounterpartyInfoAsync(movement, cancellationToken);
                var identification = counterpartyInfo.identification;
                var verificationDigit = counterpartyInfo.verificationDigit;
                var name = counterpartyInfo.name;
                var natureValue = EnumHelper.GetEnumMemberValue(movement.TreasuryConcept?.Nature);
                var concept = movement.TreasuryConcept?.Concept ?? string.Empty;

                // 2. Validar cuentas contables para todas las cuentas del portafolio
                if (!ValidateAccountingAccounts(movement.PortfolioId, concept, accountTreasuries, accountConcepts, out var accountValidationErrors))
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
                    movement.TreasuryConcept?.Observations ?? string.Empty,
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
            int portfolioId,
            string concept,
            IReadOnlyCollection<GetAccountingConceptsTreasuriesResponse> accountTreasuries,
            IReadOnlyCollection<GetConceptsByPortfolioIdsResponse> accountConcepts,
            out List<AccountingInconsistency> validationErrors)
        {
            try
            {
                validationErrors = new List<AccountingInconsistency>();
                bool isValid = true;

                // Validar que existan cuentas de treasury para el portafolio
                if (accountTreasuries == null || !accountTreasuries.Any())
                {
                    logger.LogWarning("No se encontraron conceptos de cuentas de treasury para el portafolio {PortfolioId}", portfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe parametrización contable de treasury", AccountingActivity.Debit));
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe parametrización contable de treasury", AccountingActivity.Credit));
                    isValid = false;
                }
                else
                {
                    // Validar cada cuenta de treasury
                    foreach (var accountTreasury in accountTreasuries)
                    {
                        if (string.IsNullOrWhiteSpace(accountTreasury.DebitAccount))
                        {
                            logger.LogWarning("No se encontró cuenta de débito de treasury para el portafolio {PortfolioId}", portfolioId);
                            validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe cuenta de débito de treasury", AccountingActivity.Debit));
                            isValid = false;
                        }

                        if (string.IsNullOrWhiteSpace(accountTreasury.CreditAccount))
                        {
                            logger.LogWarning("No se encontró cuenta de crédito de treasury para el portafolio {PortfolioId}", portfolioId);
                            validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe cuenta de crédito de treasury", AccountingActivity.Credit));
                            isValid = false;
                        }
                    }
                }

                // Validar que existan cuentas de concepto para el portafolio
                if (accountConcepts == null || !accountConcepts.Any())
                {
                    logger.LogWarning("No se encontraron conceptos de cuentas para el portafolio {PortfolioId}", portfolioId);
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe parametrización contable de conceptos", AccountingActivity.Debit));
                    validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe parametrización contable de conceptos", AccountingActivity.Credit));
                    isValid = false;
                }
                else
                {
                    // Validar cada cuenta de concepto
                    foreach (var accountConcept in accountConcepts)
                    {
                        if (string.IsNullOrWhiteSpace(accountConcept.DebitAccount))
                        {
                            logger.LogWarning("No se encontró cuenta de débito de concepto para el portafolio {PortfolioId}", portfolioId);
                            validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe cuenta de débito de concepto", AccountingActivity.Debit));
                            isValid = false;
                        }

                        if (string.IsNullOrWhiteSpace(accountConcept.CreditAccount))
                        {
                            logger.LogWarning("No se encontró cuenta de crédito de concepto para el portafolio {PortfolioId}", portfolioId);
                            validationErrors.Add(AccountingInconsistency.Create(portfolioId, $"{OperationTypeNames.Concepts} - {concept}", "No existe cuenta de crédito de concepto", AccountingActivity.Credit));
                            isValid = false;
                        }
                    }
                }

                return isValid;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al validar cuentas contables para el portafolio {PortfolioId}", portfolioId);
                validationErrors = new List<AccountingInconsistency>();
                return false;
            }
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