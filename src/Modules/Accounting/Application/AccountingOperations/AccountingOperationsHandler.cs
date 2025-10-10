using Accounting.Application.Abstractions;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Associate.IntegrationsEvents.GetActivateIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Customers.IntegrationEvents.PeopleByIdentificationsValidation;
using Customers.Integrations.People.GetPeopleByIdentifications;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.ClientOperations;
using System.Collections.Concurrent;

namespace Accounting.Application.AccountingOperations
{
    internal sealed class AccountingOperationsHandler(
        IServiceScopeFactory serviceScopeFactory,
        IRpcClient rpcClient,
        ILogger<AccountingOperationsHandler> logger,
        AccountingOperationsHandlerValidation validator,
        IInconsistencyHandler inconsistencyHandler) : ICommandHandler<AccountingOperationsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                await using var scope = serviceScopeFactory.CreateAsyncScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();
                var errors = new ConcurrentBag<AccountingInconsistency>();

                //Operations
                var operations = await rpcClient.CallAsync<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>(
                                                                new GetAccountingOperationsRequestEvents(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!operations.IsValid)
                    return Result.Failure<bool>(Error.Validation(operations.Code ?? string.Empty, operations.Message ?? string.Empty));
                var operationsByPortfolio = operations.ClientOperations.GroupBy(op => op.PortfolioId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var portfolioId in command.PortfolioIds)
                {
                    if (!operationsByPortfolio.ContainsKey(portfolioId) || !operationsByPortfolio[portfolioId].Any())
                    {
                        logger.LogInformation($"No hay operaciones contables para el PortfolioId: {portfolioId}");
                        errors.Add(AccountingInconsistency.Create(portfolioId, OperationTypeNames.Commission, "No se encontraron operaciones contables para el portfolio", string.Empty));
                    }
                }

                if (errors.Any())
                {
                    await inconsistencyHandler.HandleInconsistenciesAsync(errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                    return false;
                }

                //Identifications
                var activateIds = operations.ClientOperations.GroupBy(x => x.AffiliateId).Select(x => x.Key).ToList();
                var identificationResult = await rpcClient.CallAsync<GetIdentificationByActivateIdsRequestEvent, GetIdentificationByActivateIdsResponseEvent>(
                                                                new GetIdentificationByActivateIdsRequestEvent(activateIds), cancellationToken);
                if (!identificationResult.IsValid)
                    return Result.Failure<bool>(Error.Validation(identificationResult.Code ?? string.Empty, identificationResult.Message ?? string.Empty));
                var identificationByActivateId = identificationResult.Indentifications.ToDictionary(x => x.ActivateIds, x => x.Identification);

                //People
                var identifications = identificationResult.Indentifications.GroupBy(x => x.Identification).Select(x => x.Key).ToList();
                var people = await rpcClient.CallAsync<GetPersonByIdentificationsRequestEvent, GetPeopleByIdentificationsResponseEvent>(
                                                                new GetPersonByIdentificationsRequestEvent(identifications), cancellationToken);
                if (!people.IsValid)
                    return Result.Failure<bool>(Error.Validation(people.Code ?? string.Empty, people.Message ?? string.Empty));
                var peopleByIdentification = people.Person?.ToDictionary(x => x.Identification, x => x) ?? new Dictionary<string, GetPeopleByIdentificationsResponse>();

                //Treasury
                var collectionAccount = operations.ClientOperations.GroupBy(x => x.CollectionAccount).Select(x => x.Key).ToList();
                var treasury = await sender.Send(new GetAccountingOperationsTreasuriesQuery(command.PortfolioIds, collectionAccount), cancellationToken);
                if (!treasury.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));
                var treasuryByPortfolioId = treasury.Value.ToDictionary(x => x.PortfolioId, x => x);

                //PassiveTransaction
                var operationTypeId = operations.ClientOperations.GroupBy(x => x.OperationTypeId).Select(x => x.Key).ToList();
                var passiveTransaction = await sender.Send(new GetAccountingOperationsPassiveTransactionQuery(command.PortfolioIds, operationTypeId), cancellationToken);
                if (!treasury.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener las cuentas" ?? string.Empty, passiveTransaction.Description ?? string.Empty));
                var passiveTransactionByPortfolioId = passiveTransaction.Value.ToDictionary(x => x.PortfolioId, x => x);

                var accountingAssistants = await validator.ProcessOperationsInParallel(
                    operations.ClientOperations,
                    identificationByActivateId,
                    peopleByIdentification,
                    treasuryByPortfolioId,
                    passiveTransactionByPortfolioId,
                    command.ProcessDate,
                    cancellationToken);

                if (!accountingAssistants.IsSuccess)
                {
                    logger.LogInformation("Insertar errores en Redis");
                    await inconsistencyHandler.HandleInconsistenciesAsync(accountingAssistants.Errors, command.ProcessDate, ProcessTypes.AccountingOperations, cancellationToken);
                    return false;
                }

                await sender.Send(new AddAccountingEntitiesCommand(accountingAssistants.SuccessItems), cancellationToken);

                return Result.Success<bool>(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling GetAccountingFeesQuery");
                return false;
            }
        }
    }
}
