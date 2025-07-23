
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.TimeControl;

public interface ITimeControlService
{
    Task<Result> StartAsync(int portfolioId, DateTime startDate, CancellationToken cancellationToken);
    Task UpdateStepAsync(int portfolioId, string process, DateTime processDatetime, CancellationToken cancellationToken);
    Task EndAsync(int portfolioId, CancellationToken cancellationToken);
}

