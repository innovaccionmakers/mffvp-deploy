using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries;
using Accounting.Integrations.Treasuries.GetConceptsByPortfolioIds;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Products.IntegrationEvents.Portfolio.GetPortfolioInformation;
using Treasury.Domain.TreasuryMovements;

namespace Accounting.Application.AccountingConcepts
{
    public record class AccountingConceptsHandlerValidator(
        IRpcClient rpcClient,
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

                if (movement.CounterpartyId == null || movement.CounterpartyId == 0)
                {
                    var portfolio = await rpcClient.CallAsync<GetPortfolioInformationByIdRequest, GetPortfolioInformationByIdResponse>(
                                                       new GetPortfolioInformationByIdRequest(movement.PortfolioId), cancellationToken);

                    identification = portfolio.PortfolioInformation.PortfolioNIT;
                    verificationDigit = portfolio.PortfolioInformation?.VerificationDigit ?? 0;
                    name = portfolio.PortfolioInformation.Name;
                }
                else
                {
                    identification = movement.Counterparty.Nit;
                    verificationDigit = movement.Counterparty?.Digit ?? 0;
                    name = movement.Counterparty.Description;
                }

                bool isTreasuryConceptZero = movement.TreasuryConceptId == 0;
                bool requiresBankAccount = movement.TreasuryConcept.RequiresBankAccount == true;

                if (isTreasuryConceptZero)
                {
                    debitAccount = requiresBankAccount
                        ? accountTreasury?.DebitAccount ?? string.Empty
                        : accountConcept?.DebitAccount ?? string.Empty;

                    creditAccount = accountConcept?.CreditAccount ?? string.Empty;
                }
                else
                {
                    debitAccount = accountConcept?.DebitAccount ?? string.Empty;

                    creditAccount = requiresBankAccount
                        ? accountTreasury?.CreditAccount ?? string.Empty
                        : accountConcept?.CreditAccount ?? string.Empty;
                }

                var accountingAssistant = AccountingAssistant.Create(
                    identification ?? string.Empty,
                    verificationDigit,
                    name ?? string.Empty,
                    command.ProcessDate.ToString("yyyyMM"),
                    command.ProcessDate,
                    movement.TreasuryConcept.Observations ?? string.Empty,
                    movement.Value,
                    movement.TreasuryConcept.Nature.ToString() ?? string.Empty
                    );

                if (accountingAssistant.IsFailure)
                {
                    logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}", movement.PortfolioId, accountingAssistant.Error);
                    errors.Add(AccountingInconsistency.Create(movement.PortfolioId, OperationTypeNames.Concepts, accountingAssistant.Error.Description));
                    continue;
                }

                accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(debitAccount, creditAccount));
            }

            return new ProcessingResult<AccountingAssistant, AccountingInconsistency>(accountingAssistants, errors);
        }
    }
}
