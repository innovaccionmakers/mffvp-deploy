using Closing.IntegrationEvents;
using DotNetCore.CAP;
using MediatR;
using Products.Integrations.TechnicalSheets.Commands;

namespace Products.IntegrationEvents.TechnicalSheet;

public sealed class TechnicalSheetDataBuilderSuscriber(ISender sender) : ICapSubscribe
{
    [CapSubscribe(nameof(TechnicalSheetDataBuilderEvent))]
    public async Task HandleAsync(
        TechnicalSheetDataBuilderEvent message,
        CancellationToken cancellationToken)
    {
       await sender.Send(new SaveTechnicalSheetCommand(message.ClosingDate), cancellationToken);
    }
}
