
namespace Closing.Application.PostClosing.Services.DailyDataLoad;

public interface IDailyDataPublisher
{
    Task PublishAsync(int portfolioId, DateTime closingDatetime, CancellationToken cancellationToken);
}