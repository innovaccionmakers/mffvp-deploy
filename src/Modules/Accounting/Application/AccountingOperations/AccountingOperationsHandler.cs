using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.Treasuries.GetTreasuriesByPortfolioIds;
using Associate.IntegrationsEvents.GetActivateIds;
using Azure;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Customers.Domain.People;
using Customers.IntegrationEvents.PeopleByIdentificationsValidation;
using Customers.Integrations.People.GetPeopleByIdentifications;
using MediatR;
using Operations.IntegrationEvents.ClientOperations;

namespace Accounting.Application.AccountingOperations
{
    internal sealed class AccountingOperationsHandler(
        ISender sender,
        IRpcClient rpcClient) : ICommandHandler<AccountingOperationsCommand, string>
    {
        public async Task<Result<string>> Handle(AccountingOperationsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var operations = await rpcClient.CallAsync<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>(
                                                                new GetAccountingOperationsRequestEvents(command.PortfolioIds, command.ProcessDate), cancellationToken);
                if (!operations.IsValid)
                    return Result.Failure<string>(Error.Validation(operations.Code ?? string.Empty, operations.Message ?? string.Empty));

                var activateIds = operations.ClientOperations.GroupBy(x => x.AffiliateId).Select(x => x.Key).ToList();
                var identification = await rpcClient.CallAsync<GetIdentificationByActivateIdsRequestEvent, GetIdentificationByActivateIdsResponseEvent>(
                                                                new GetIdentificationByActivateIdsRequestEvent(activateIds), cancellationToken);
                if (!identification.IsValid)
                    return Result.Failure<string>(Error.Validation(identification.Code ?? string.Empty, identification.Message ?? string.Empty));

                var identifications = identification.Indentifications.GroupBy(x => x.Identification).Select(x => x.Key).ToList();
                var peoples = await rpcClient.CallAsync<GetPersonByIdentificationsRequestEvent, GetPeopleByIdentificationsResponseEvent>(
                                                                new GetPersonByIdentificationsRequestEvent(identifications), cancellationToken);
                if (!peoples.IsValid)
                    return Result.Failure<string>(Error.Validation(peoples.Code ?? string.Empty, peoples.Message ?? string.Empty));

                var treasury = await sender.Send(new GetTreasuriesByPortfolioIdsQuery(command.PortfolioIds), cancellationToken);

                var identificationByActivateId = identification.Indentifications
                     .ToDictionary(x => x.ActivateIds, x => x.Identification);

                var peopleByIdentification = peoples.Person?
                    .ToDictionary(x => x.Identification, x => x)
                    ?? new Dictionary<string, GetPeopleByIdentificationsResponse>();

                var treasuryByPortfolioId = treasury.Value
                    .ToDictionary(x => x.PortfolioIds, x => x);

                // 6. Transformar cada operación en AccountingOperationsResponse
                var accountingOperations = operations.ClientOperations.Select(operation =>
                {
                    // Obtener identificación del affiliate
                    var identification = identificationByActivateId.GetValueOrDefault(operation.AffiliateId);

                    // Obtener información de la persona
                    var person = peopleByIdentification.GetValueOrDefault(identification);

                    // Obtener información de tesorería
                    var treasury = treasuryByPortfolioId.GetValueOrDefault(operation.PortfolioId);

                    return new AccountingOperationsResponse(
                        Identification: identification ?? string.Empty,
                        VerificationDigit: 0,
                        Name: person?.FullName ?? string.Empty,
                        Period: command.ProcessDate.ToString("yyyyMM"),
                        Account: treasury?.DebitAccount ?? string.Empty,
                        Date: command.ProcessDate,
                        Detail: operation.OperationType,
                        Type: "D",
                        Value: operation.Amount,
                        Nature: operation.OperationType,
                        Nit: string.Empty,
                        Identifier: 0                        
                    );
                }).ToList();

                return Result.Success<string>(string.Empty);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
