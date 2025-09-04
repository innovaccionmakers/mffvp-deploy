using Common.SharedKernel.Application.EventBus;

namespace Closing.IntegrationEvents;

public sealed class TechnicalSheetDataBuilderEvent : IntegrationEvent
{
    public TechnicalSheetDataBuilderEvent(DateOnly closingDate) : base(Guid.NewGuid(), DateTime.UtcNow)
    {
        ClosingDate = closingDate;
    }

    public DateOnly ClosingDate { get; init; }
}
