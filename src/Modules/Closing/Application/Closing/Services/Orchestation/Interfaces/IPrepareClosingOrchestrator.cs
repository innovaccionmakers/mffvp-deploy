using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.Orchestation.Interfaces;

public interface IPrepareClosingOrchestrator
{
    Task<Result<PrepareClosingResult>> PrepareAsync(PrepareClosingCommand command, CancellationToken cancellationToken);
}