using Accounting.Domain.AccountingAssistants;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.Treasuries.GetConceptsByPortfolioIds;
using Accounting.Integrations.Treasuries.GetTreasuriesByPortfolioIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.IntegrationEvents.Portfolio.GetPortfolioInformation;
using Treasury.IntegrationEvents.TreasuryMovements.AccountingConcepts;

namespace Accounting.Application.AccountingConcepts
{
    internal class AccountingConceptsHandler(
        ISender sender,
        IRpcClient rpcClient,
        ILogger<AccountingConceptsHandler> logger) : ICommandHandler<AccountingConceptsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AccountingConceptsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var accountingAssistants = new List<AccountingAssistant>();

                //TreasuryMovement
                var treasuryMovement = await rpcClient.CallAsync<AccountingConceptsRequestEvent, AccountingConceptsResponseEvent>(
                                                                new AccountingConceptsRequestEvent(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!treasuryMovement.IsValid)
                    return Result.Failure<bool>(Error.Validation(treasuryMovement.Code ?? string.Empty, treasuryMovement.Message ?? string.Empty));

                //Treasury
                var treasury = await sender.Send(new GetTreasuriesByPortfolioIdsQuery(command.PortfolioIds), cancellationToken);
                if (!treasury.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));
                var treasuryByPortfolioId = treasury.Value.ToDictionary(x => x.PortfolioId, x => x);


                var concept = await sender.Send(new GetConceptsByPortfolioIdsQuery(command.PortfolioIds), cancellationToken);
                if (!concept.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener los conceptos" ?? string.Empty, concept.Description ?? string.Empty));
                var conceptByPortfolioId = concept.Value.ToDictionary(x => x.PortfolioId, x => x);

                foreach (var movement in treasuryMovement.movements)
                {
                    var errors = new List<Error>();
                    var identification = string.Empty;
                    int verificationDigit = 0;
                    string name = string.Empty;
                    var accountTreasury = treasuryByPortfolioId.GetValueOrDefault(movement.PortfolioId);
                    var accountConcept = conceptByPortfolioId.GetValueOrDefault(movement.PortfolioId);
                    string debitAccount = string.Empty;
                    string creditAccount = string.Empty;

                    if (movement.CounterpartyId == null || movement.CounterpartyId == 0)
                    {
                        var portfolio = await rpcClient.CallAsync<GetPortfolioInformationByIdRequest, GetPortfolioInformationByIdResponse>(
                                                           new GetPortfolioInformationByIdRequest(movement.PortfolioId), cancellationToken);
                        if (!portfolio.Succeeded)
                            return Result.Failure<bool>(Error.Validation("Error al optener los portfolios" ?? string.Empty, treasury.Description ?? string.Empty));

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

                    if (movement.TreasuryConceptId == 0)
                    {
                        if (movement.TreasuryConcept.RequiresBankAccount == true)
                        {
                            debitAccount = accountTreasury?.DebitAccount ?? string.Empty;
                            creditAccount = accountConcept?.CreditAccount ?? string.Empty;
                        }
                        else
                        {
                            debitAccount = accountConcept?.ContraCreditAccount ?? string.Empty;
                            creditAccount = accountConcept?.CreditAccount ?? string.Empty;
                        }
                    }
                    else
                    {
                        if (movement.TreasuryConcept.RequiresBankAccount == true)
                        {
                            debitAccount = accountConcept?.DebitAccount ?? string.Empty;
                        }
                        else
                        {
                            debitAccount = accountConcept?.DebitAccount ?? string.Empty;
                            creditAccount = accountConcept?.ContraDebitAccount ?? string.Empty;
                        }
                    }

                    var accountingAssistant = AccountingAssistant.Create(
                        identification ?? string.Empty,
                        verificationDigit,
                        name ?? string.Empty,
                        command.ProcessDate.ToString("yyyyMM"),
                        debitAccount,
                        command.ProcessDate,
                        movement.TreasuryConcept.Observations ?? string.Empty,                        
                        movement.Value,
                        movement.TreasuryConcept.Nature.ToString() ?? string.Empty
                        );

                    if (accountingAssistant.IsFailure)
                    {
                        logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}", movement.PortfolioId, accountingAssistant.Error);
                        errors.Add(accountingAssistant.Error);
                    }

                    accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit(debitAccount, creditAccount));
                }

                return Result.Success<bool>(true);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
