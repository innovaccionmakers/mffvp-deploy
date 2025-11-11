using Accounting.Application.Abstractions;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.ClientOperations;
using System.Collections.Concurrent;

namespace Accounting.Application.AccountingOperations
{
    internal sealed class AccountingOperationsHandler(
        IRpcClient rpcClient,
        ISender sender,
        ILogger<AccountingOperationsHandler> logger,
        AccountingOperationsHandlerValidation validator,
        IInconsistencyHandler inconsistencyHandler) : ICommandHandler<AccountingOperationsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var errors = new ConcurrentBag<AccountingInconsistency>();

                //Operations
                var operations = await rpcClient.CallAsync<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>(
                                                                new GetAccountingOperationsRequestEvents(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!operations.IsValid)
                    return Result.Failure<bool>(Error.Validation(operations.Code ?? string.Empty, operations.Message ?? string.Empty));

                if (operations.ClientOperations.Count == 0)
                    return Result.Success(true);

                var operationsByPortfolio = operations.ClientOperations.GroupBy(op => op.PortfolioId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var portfolioId in command.PortfolioIds)
                {
                    if (!operationsByPortfolio.ContainsKey(portfolioId) || !operationsByPortfolio[portfolioId].Any())
                    {
                        logger.LogInformation($"No hay operaciones contables para el PortfolioId: {portfolioId}");
                        errors.Add(AccountingInconsistency.Create(portfolioId, OperationTypeNames.Operation, "No se encontraron operaciones contables para el portfolio", string.Empty));
                    }
                }

                if (errors.Any())
                {
                    await inconsistencyHandler.HandleInconsistenciesAsync(errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                    return Result.Failure<bool>(Error.Problem("Accounting.Operations", "Se encontraron inconsistencias"));
                }

                //Treasury
                var collectionAccount = operations.ClientOperations.GroupBy(x => x.CollectionAccount).Select(x => x.Key).ToList();
                var treasury = await sender.Send(new GetAccountingOperationsTreasuriesQuery(command.PortfolioIds, collectionAccount), cancellationToken);
                if (!treasury.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));
                var treasuryByPortfolioId = treasury.Value.ToDictionary(x => x.PortfolioId, x => x);

                var accountingAssistants = await validator.ProcessOperationsInParallel(
                    operations.ClientOperations,
                    treasuryByPortfolioId,
                    command,
                    cancellationToken);

                if (!accountingAssistants.IsSuccess)
                {
                    logger.LogInformation("Insertar errores en Redis");
                    await inconsistencyHandler.HandleInconsistenciesAsync(accountingAssistants.Errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                    return Result.Failure<bool>(Error.Problem("Accounting.Operations", "Se encontraron inconsistencias"));
                }

                if (!accountingAssistants.SuccessItems.Any())
                {
                    logger.LogInformation("No hay operaciones contables que procesar");
                    return Result.Failure<bool>(Error.Problem("Accounting.Operations", "No hay operaciones contables que procesar"));
                }

                var accountingOperationsSave =  await sender.Send(new AddAccountingEntitiesCommand(accountingAssistants.SuccessItems), cancellationToken);

                if (accountingOperationsSave.IsFailure)
                {
                    logger.LogWarning("No se pudieron guardar las operaciones contables: {Error}", accountingOperationsSave.Error);
                    return Result.Failure<bool>(Error.Problem("Accounting.Operations", "No se pudieron guardar las operaciones contables"));
                }

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar las operaciones contables para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                     command.ProcessDate,
                     string.Join(",", command.PortfolioIds)
                 );
                return Result.Failure<bool>(Error.Problem("Exception", "Ocurrio un error inesperado al procesar las operaciones contables"));
            }
        }
    }
}
