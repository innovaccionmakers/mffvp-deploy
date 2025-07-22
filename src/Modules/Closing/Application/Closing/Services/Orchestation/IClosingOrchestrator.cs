using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.Orchestation;

public interface IClosingOrchestrator
{
    Task<Result<ClosedResult>> RunClosingAsync(RunClosingCommand command, CancellationToken cancellationToken);
}