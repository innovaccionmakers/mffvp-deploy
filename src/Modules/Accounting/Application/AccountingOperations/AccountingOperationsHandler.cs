using Accounting.Domain.AccountingAssistants;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.Treasuries.GetTreasuriesByPortfolioIds;
using Associate.IntegrationsEvents.GetActivateIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Customers.IntegrationEvents.PeopleByIdentificationsValidation;
using Customers.Integrations.People.GetPeopleByIdentifications;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.IntegrationEvents.ClientOperations;

namespace Accounting.Application.AccountingOperations
{
    internal sealed class AccountingOperationsHandler(
        ISender sender,
        IRpcClient rpcClient,
        ILogger<AccountingOperationsHandler> logger) : ICommandHandler<AccountingOperationsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var accountingAssistants = new List<AccountingAssistant>();
                //Operations
                var operations = await rpcClient.CallAsync<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>(
                                                                new GetAccountingOperationsRequestEvents(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!operations.IsValid)
                    return Result.Failure<bool>(Error.Validation(operations.Code ?? string.Empty, operations.Message ?? string.Empty));
                
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
                var treasury = await sender.Send(new GetTreasuriesByPortfolioIdsQuery(command.PortfolioIds), cancellationToken);
                if (!treasury.IsSuccess)
                    return Result.Failure<bool>(Error.Validation("Error al optener las cuentas" ?? string.Empty, treasury.Description ?? string.Empty));
                var treasuryByPortfolioId = treasury.Value.ToDictionary(x => x.PortfolioId, x => x);

                foreach (var operation in operations.ClientOperations)
                {
                    var errors = new List<Error>();
                    var identification = identificationByActivateId.GetValueOrDefault(operation.AffiliateId);
                    var person = peopleByIdentification!.GetValueOrDefault(identification);
                    var debitAccount = treasuryByPortfolioId.GetValueOrDefault(operation.PortfolioId);

                    var accountingAssistant = AccountingAssistant.Create(
                        identification ?? string.Empty,
                        0,
                        person?.FullName ?? string.Empty,
                        command.ProcessDate.ToString("yyyyMM"),
                        debitAccount?.DebitAccount ?? string.Empty,
                        command.ProcessDate,
                        operation.OperationType,                        
                        operation.Amount,
                        operation.OperationType
                        );

                    if (accountingAssistant.IsFailure)
                    {
                        logger.LogError("Error al crear el AccountingAssistant para el portafolio {PortfolioId}: {Error}", operation.PortfolioId, accountingAssistant.Error);
                        errors.Add(accountingAssistant.Error);
                    }

                    accountingAssistants.AddRange(accountingAssistant.Value.ToDebitAndCredit());
                }

                return await sender.Send(new AddAccountingEntitiesCommand(accountingAssistants), cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling GetAccountingFeesQuery");
                return false;
            }
        }
    }
}
