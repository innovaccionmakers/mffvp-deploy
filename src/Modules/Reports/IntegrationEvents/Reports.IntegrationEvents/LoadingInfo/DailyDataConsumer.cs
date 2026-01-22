

using DotNetCore.CAP;
using MediatR;
using Reports.Integrations.LoadingInfo.Commands;
using Closing.IntegrationEvents.DailyDataLoad;

namespace Reports.IntegrationEvents.LoadingInfo;

public sealed class DailyDataConsumer : ICapSubscribe
{
    private readonly ISender _mediator;


    public DailyDataConsumer(ISender mediator)
    {
        _mediator = mediator;
    }
    [CapSubscribe(nameof(DailyDataIntegrationEvent))]
    public async Task HandleAsync(DailyDataIntegrationEvent integrationEvent, CancellationToken cancellationToken)
    {
        var command = new ProcessDailyDataCommand(
            integrationEvent.PortfolioId,
            integrationEvent.ClosingDatetime,
            Domain.LoadingInfo.Constants.EtlSelection.All);
        await _mediator.Send(command, cancellationToken);
    }
}
