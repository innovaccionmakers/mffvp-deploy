using Common.SharedKernel.Application.Caching.Closing;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Domain;
using DotNetCore.CAP;

namespace Closing.Application.Closing.Services.TimeControl;

public class TimeControlService(
    IClosingExecutionStore store,
    ICapPublisher capPublisher)
    : ITimeControlService
{
    public async Task<Result> StartAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        // Verifica si ya hay un proceso de cierre activo
        var isActive = await store.IsClosingActiveAsync(portfolioId, cancellationToken);
        if (isActive)
        {
            return Result.Failure(new Error("0001","Ya existe un proceso de cierre activo para el portafolio.", ErrorType.Validation));
        }

        var now = DateTime.UtcNow;

        // Guarda el estado inicial en Redis
        await store.BeginAsync(portfolioId, now, cancellationToken);

        //// Publica evento inicial
        //await capPublisher.PublishAsync("ClosingStepEvent", new
        //{
        //    PortfolioId = portfolioId,
        //    ClosingDatetime = now,
        //    ProcessDatetime = now,
        //    Process = ClosingProcess.Begin.ToString()
        //}, cancellationToken);

        return Result.Success();
    }

    public async Task UpdateStepAsync(int portfolioId, string process, DateTime processDatetime, CancellationToken cancellationToken)
    {
        if (process == ClosingProcess.End)
        {
            await store.EndAsync(portfolioId, cancellationToken);
        }
        else
        {
            
            await store.UpdateProcessAsync(portfolioId, process, cancellationToken);
         
        }
    }

    public async Task EndAsync(int portfolioId, CancellationToken cancellationToken)
    {
        await store.EndAsync(portfolioId, cancellationToken);

        //await capPublisher.PublishAsync("ClosingStepEvent", new
        //{
        //    PortfolioId = portfolioId,
        //    ClosingDatetime = (DateTime?)null,
        //    ProcessDatetime = DateTime.UtcNow,
        //    Process = ClosingProcess.End.ToString()
        //}, cancellationToken);
    }
}