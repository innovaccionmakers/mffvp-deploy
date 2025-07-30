
namespace Closing.Application.Closing.Services.TimeControl.Interrfaces;

public interface IClosingStepEventPublisher
{
    Task PublishAsync(int portfolioId, string process, DateTime closingDatetime, DateTime processDatetime, CancellationToken cancellationToken);
}
