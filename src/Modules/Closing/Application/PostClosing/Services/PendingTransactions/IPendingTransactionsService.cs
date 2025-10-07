namespace Closing.Application.PostClosing.Services.PendingTransactions;
public interface IPendingTransactionsService
{
    Task HandleAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}