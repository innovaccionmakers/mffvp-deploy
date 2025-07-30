
namespace Closing.Application.PostClosing.Services.Orchestation;
public interface IPostClosingEventsOrchestation
{
    Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}