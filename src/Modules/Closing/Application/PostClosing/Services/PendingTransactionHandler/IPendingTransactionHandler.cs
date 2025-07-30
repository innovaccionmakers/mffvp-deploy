namespace Closing.Application.PostClosing.Services.PendingTransactionHandler;
public interface IPendingTransactionHandler
{
    Task HandleAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}