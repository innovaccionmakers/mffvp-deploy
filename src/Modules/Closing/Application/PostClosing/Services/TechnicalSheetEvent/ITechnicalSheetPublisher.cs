namespace Closing.Application.PostClosing.Services.TechnicalSheetEvent;

public interface ITechnicalSheetPublisher
{
    Task PublishAsync(DateOnly closingDate, CancellationToken cancellationToken);
}
