namespace Closing.Application.PostClosing.Services.TrustYieldEvent;

public interface ITrustYieldPublisher
{
    Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}