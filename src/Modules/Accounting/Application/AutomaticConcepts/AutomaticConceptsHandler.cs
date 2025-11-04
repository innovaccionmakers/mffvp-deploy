using Accounting.Application.Abstractions;
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
        ILogger<AutomaticConceptsHandler> logger,
        IInconsistencyHandler inconsistencyHandler) : ICommandHandler<AutomaticConceptsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AutomaticConceptsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var automaticConcept = "Concepto Automático";

                //Yield
                var yieldResult = await rpcClient.CallAsync<GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest, GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse>(
                                                                new GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!yieldResult.IsValid)
                    return Result.Success(true);                

                //OperationType
                var operationsType = await rpcClient.CallAsync<GetOperationTypeByNameRequest, GetOperationTypeByNameResponse>(
                                                    new GetOperationTypeByNameRequest(automaticConcept), cancellationToken);
                if (!operationsType.Succeeded)
                    return Result.Failure<bool>(Error.Validation(operationsType.Code ?? string.Empty, operationsType.Message ?? string.Empty));

                var automaticConcepts = await validator.AutomaticConceptsValidator(command, yieldResult, operationsType, automaticConcept, cancellationToken);

                if (!automaticConcepts.IsSuccess)
                {
                    logger.LogInformation("Insertar errores en Redis");
                    await inconsistencyHandler.HandleInconsistenciesAsync(automaticConcepts.Errors, command.ProcessDate, ProcessTypes.AutomaticConcepts, cancellationToken);
                    return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "Se encontraron inconsistencias"));
                }

                if (!automaticConcepts.SuccessItems.Any())
                {
                    logger.LogInformation("No hay operaciones contables que procesar");
                    return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "No hay conceptos automáticos que procesar"));
                }

                var automaticConceptsSave = await sender.Send(new AddAccountingEntitiesCommand(automaticConcepts.SuccessItems), cancellationToken);

                if (automaticConceptsSave.IsFailure)
                {
                    logger.LogWarning("No se pudieron guardar las operacines contables: {Error}", automaticConceptsSave.Error);
                    return Result.Failure<bool>(Error.Problem("Automatic.Concepts", "No se pudieron guardar los conceptos automáticos"));
                }

                return Result.Success(true);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
