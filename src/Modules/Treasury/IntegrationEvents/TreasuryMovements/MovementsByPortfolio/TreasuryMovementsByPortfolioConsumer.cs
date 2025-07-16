using Common.SharedKernel.Application.Rpc;
using MediatR;
using Treasury.Integrations.TreasuryMovements.Queries;
using Treasury.Integrations.TreasuryMovements.Response;

namespace Treasury.IntegrationEvents.TreasuryMovements.TreasuryMovementsByPortfolio;

public sealed class TreasuryMovementsByPortfolioConsumer : IRpcHandler<TreasuryMovementsByPortfolioRequest, TreasuryMovementsByPortfolioResponse>
{
    private readonly ISender _mediator;
    public TreasuryMovementsByPortfolioConsumer(ISender mediator) => _mediator = mediator;

    public async Task<TreasuryMovementsByPortfolioResponse> HandleAsync(
    TreasuryMovementsByPortfolioRequest request,
    CancellationToken cancellationToken)
    {
        var treasuryMovementsResult = await _mediator.Send(
       new GetMovementsByPortfolioIdQuery(request.PortfolioId, request.ClosingDate),
       cancellationToken);

        return treasuryMovementsResult.Match(
          val => new TreasuryMovementsByPortfolioResponse(
              true,
              null,
              null,
              val),
          error => new TreasuryMovementsByPortfolioResponse(
              false,
              error.Code,
              error.Description,
              Array.Empty<GetMovementsByPortfolioIdResponse>())
         );

        }
    }
