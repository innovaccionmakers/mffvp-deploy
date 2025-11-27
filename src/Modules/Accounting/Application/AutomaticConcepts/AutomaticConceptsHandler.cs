using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AutomaticConcepts;
using Closing.IntegrationEvents.Yields;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.OperationTypes;

namespace Accounting.Application.AutomaticConcepts
{
    internal sealed class AutomaticConceptsHandler(
        AutomaticConceptsHandlerValidator validator,
        ISender sender,
        IRpcClient rpcClient,
        IOperationLocator operationLocator,
        ILogger<AutomaticConceptsHandler> logger,
        IInconsistencyHandler inconsistencyHandler) : ICommandHandler<AutomaticConceptsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AutomaticConceptsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                //Yield
                var yieldResult = await rpcClient.CallAsync<GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest, GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse>(
                                                                new GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!yieldResult.IsValid)
                    return Result.Success(true);

                //OperationType
                var operationTypes = await operationLocator.GetOperationTypesByNameAsync(OperationTypeNames.AutomaticConcept, cancellationToken);
                
                if (operationTypes.IsFailure)
                    return Result.Failure<bool>(Error.Validation(operationTypes.Error.Code ?? string.Empty, operationTypes.Error.Description ?? string.Empty));

                var automaticConcepts = await validator.AutomaticConceptsValidator(command.ProcessDate, yieldResult, operationTypes.Value, OperationTypeNames.AutomaticConcept, cancellationToken);

                if (!automaticConcepts.IsSuccess)
                {
                    logger.LogInformation("Insertar errores en Redis");
                    await inconsistencyHandler.HandleInconsistenciesAsync(automaticConcepts.Errors, command.ProcessDate, ProcessTypes.AutomaticConcepts, cancellationToken);
                    return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "Se encontraron inconsistencias"));
                }

                if (!automaticConcepts.SuccessItems.Any())
                {
                    logger.LogInformation("No hay conceptos automáticos que procesar");
                    return Result.Success(true);
                }

                var automaticConceptsSave = await sender.Send(new AddAccountingEntitiesCommand(automaticConcepts.SuccessItems), cancellationToken);

                if (automaticConceptsSave.IsFailure)
                {
                    logger.LogWarning("No se pudieron guardar los conceptos automáticos: {Error}", automaticConceptsSave.Error);
                    return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "No se pudieron guardar los conceptos automáticos"));
                }

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar los conceptos automáticos para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                     command.ProcessDate,
                     string.Join(",", command.PortfolioIds)
                 );
                return Result.Failure<bool>(Error.Problem("Exception", "Ocurrio un error inesperado al procesar los conceptos automáticos"));
            }
        }
    }
}
