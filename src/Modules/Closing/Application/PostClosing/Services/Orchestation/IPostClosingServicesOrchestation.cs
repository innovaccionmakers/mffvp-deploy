
namespace Closing.Application.PostClosing.Services.Orchestation;

public interface IPostClosingServicesOrchestation
{
    Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken);
}