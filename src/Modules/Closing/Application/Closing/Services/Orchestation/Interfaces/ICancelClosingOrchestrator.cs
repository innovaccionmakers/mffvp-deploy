using Common.SharedKernel.Domain;
using Closing.Integrations.Closing.RunClosing;


namespace Closing.Application.Closing.Services.Orchestation.Interfaces;

public interface ICancelClosingOrchestrator
{
    Task<Result<ClosedResult>> CancelAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}

