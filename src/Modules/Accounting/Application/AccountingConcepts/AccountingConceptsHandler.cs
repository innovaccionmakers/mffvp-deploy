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
    internal class AccountingConceptsHandler(
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

                //Treasury
                var treasury = await sender.Send(new GetAccountingConceptsTreasuriesQuery(command.PortfolioIds), cancellationToken);
                if (!treasury.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));
                var treasuryByPortfolioId = treasury.Value.ToDictionary(x => x.PortfolioId, x => x);

                //Concept
                var concept = await sender.Send(new GetConceptsByPortfolioIdsQuery(command.PortfolioIds), cancellationToken);
                if (!concept.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener los conceptos" ?? string.Empty, concept.Description ?? string.Empty));
                var conceptByPortfolioId = concept.Value.ToDictionary(x => x.PortfolioId, x => x);
                
                var accountingAssistants = await validator.AccountingConceptsValidator(command, treasuryMovement.movements, treasuryByPortfolioId, conceptByPortfolioId, cancellationToken);

                if (!accountingAssistants.IsSuccess)
                {
                    logger.LogInformation("Insertar errores en Redis");
                    await inconsistencyHandler.HandleInconsistenciesAsync(accountingAssistants.Errors, command.ProcessDate, ProcessTypes.AccountingConcepts, cancellationToken);
                    return false;
                }

                if (!accountingAssistants.SuccessItems.Any())
                {
                    logger.LogInformation("No hay operaciones contables que procesar");
                    return false;
                }

                await sender.Send(new AddAccountingEntitiesCommand(accountingAssistants.SuccessItems), cancellationToken);

                return Result.Success<bool>(true);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
