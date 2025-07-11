using Common.SharedKernel.Application.Messaging;
using DotNetCore.CAP;
using MediatR;
using Treasury.Integrations.TreasuryMovements.Queries;
using Treasury.Integrations.TreasuryMovements.Response;

namespace Treasury.IntegrationEvents.TreasuryMovements.TreasuryMovementsByPortfolio;

public sealed class TreasuryMovementsByPortfolioConsumer : ICapSubscribe
{
    private readonly ISender _mediator;
    public TreasuryMovementsByPortfolioConsumer(ISender mediator) => _mediator = mediator;

    [CapSubscribe(nameof(TreasuryMovementsByPortfolioRequest))]
    public async Task<TreasuryMovementsByPortfolioResponse> HandleAsync(
    TreasuryMovementsByPortfolioRequest request,
    [FromCap] CapHeader header,
    CancellationToken cancellationToken)
    {
        var correlationId = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, correlationId);

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
