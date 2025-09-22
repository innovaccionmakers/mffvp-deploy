using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.Treasuries.GetTreasuriesByPortfolioIds;
using Associate.IntegrationsEvents.GetActivateIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Customers.IntegrationEvents.PeopleByIdentificationsValidation;
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

                var treasury = sender.Send(new GetTreasuriesByPortfolioIdsQuery(command.PortfolioIds), cancellationToken);

                return Result.Success<string>(string.Empty);
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
