using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.Orchestation.Interfaces;

public interface IConfirmClosingOrchestrator
{
    Task<Result<ConfirmClosingResult>> ConfirmAsync(int portfolioId, DateTime closingDate, CancellationToken ct);
}
