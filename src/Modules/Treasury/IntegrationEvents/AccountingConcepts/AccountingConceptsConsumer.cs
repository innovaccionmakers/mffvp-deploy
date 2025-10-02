using Common.SharedKernel.Application.Rpc;
using MediatR;
using Treasury.Domain.TreasuryMovements;
using Treasury.Integrations.TreasuryMovements.Queries;

namespace Treasury.IntegrationEvents.TreasuryMovements.AccountingConcepts;

public sealed class AccountingConceptsConsumer(ISender mediator) : IRpcHandler<AccountingConceptsRequestEvent, AccountingConceptsResponseEvent>
{
    public async Task<AccountingConceptsResponseEvent> HandleAsync(
        AccountingConceptsRequestEvent request,
        CancellationToken cancellationToken)
    {
        var treasuryMovementsResult = await mediator.Send( new GetAccountingConceptsQuery(request.PortfolioIds, request.ProcessDate), cancellationToken);

        return treasuryMovementsResult.Match(
          val => new AccountingConceptsResponseEvent(true, null, null, val),
          error => new AccountingConceptsResponseEvent(false, error.Code, error.Description, Array.Empty<TreasuryMovement>())
         );

        }
    }
