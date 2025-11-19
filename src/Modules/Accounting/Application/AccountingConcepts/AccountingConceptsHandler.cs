using Accounting.Application.Abstractions;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
using Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Treasury.IntegrationEvents.TreasuryMovements.AccountingConcepts;

namespace Accounting.Application.AccountingConcepts
{
    public class AccountingConceptsHandler(
        ISender sender,
        IRpcClient rpcClient,
        AccountingConceptsHandlerValidator validator,
        ILogger<AccountingConceptsHandler> logger,
        IInconsistencyHandler inconsistencyHandler) : ICommandHandler<AccountingConceptsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AccountingConceptsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                //TreasuryMovement
                var treasuryMovement = await rpcClient.CallAsync<AccountingConceptsRequestEvent, AccountingConceptsResponseEvent>(
                                                                new AccountingConceptsRequestEvent(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!treasuryMovement.IsValid)
                    return Result.Failure<bool>(Error.Validation(treasuryMovement.Code ?? string.Empty, treasuryMovement.Message ?? string.Empty));

                if (treasuryMovement.movements.Count == 0)
                    return Result.Success(true);

                var accountNumbers = treasuryMovement?.movements?
                    .Where(m => m?.BankAccount?.AccountNumber != null)
                    .Select(m => m.BankAccount.AccountNumber)
                    .ToList() ?? new List<string>();

                //Treasury
                var treasury = await sender.Send(new GetAccountingConceptsTreasuriesQuery(command.PortfolioIds, accountNumbers), cancellationToken);
                if (!treasury.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));

                var concepts = treasuryMovement?.movements?
                    .Where(m => m?.TreasuryConcept?.Concept!= null)
                    .Select(m => m.TreasuryConcept.Concept)
                    .ToList() ?? new List<string>();

                //Concept
                var concept = await sender.Send(new GetConceptsByPortfolioIdsQuery(command.PortfolioIds, concepts), cancellationToken);
                if (!concept.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener los conceptos" ?? string.Empty, concept.Description ?? string.Empty));

                var accountingAssistants = await validator.AccountingConceptsValidator(command, treasuryMovement.movements, treasury.Value, concept.Value, cancellationToken);

                if (!accountingAssistants.IsSuccess)
                {
                    logger.LogInformation("Insertar errores en Redis");
                    await inconsistencyHandler.HandleInconsistenciesAsync(accountingAssistants.Errors, command.ProcessDate, ProcessTypes.AccountingConcepts, cancellationToken);
                    return Result.Failure<bool>(Error.Problem("Accounting.Concepts", "Se encontraron inconsistencias"));
                }

                if (!accountingAssistants.SuccessItems.Any())
                {
                    logger.LogInformation("No hay operaciones contables que procesar");
                    return Result.Success(true);
                }

                var accountingConceptsSave = await sender.Send(new AddAccountingEntitiesCommand(accountingAssistants.SuccessItems), cancellationToken);

                if (accountingConceptsSave.IsFailure)
                {
                    logger.LogWarning("No se pudieron guardar las operacines contables: {Error}", accountingConceptsSave.Error);
                    return Result.Failure<bool>(Error.Problem("Accounting.Concepts", "No se pudieron guardar los conceptos contables"));
                }

                return Result.Success<bool>(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar los conceptos contables para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                    command.ProcessDate,
                    string.Join(",", command.PortfolioIds)
                );
                return Result.Failure<bool>(Error.Problem("Exception", "Ocurrio un error inesperado al procesar los conceptos contables"));
            }
        }
    }
}
