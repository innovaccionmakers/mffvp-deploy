using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Common.SharedKernel.Application.Caching.Closing;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.TimeControl;

public class TimeControlService(
    IClosingExecutionStore store,
    IClosingStepEventPublisher stepEventPublisher)
    : ITimeControlService
{
    public async Task<Result> StartAsync(int portfolioId, CancellationToken cancellationToken)
    {
        // Validación: ¿hay cierre activo?
        var isActive = await store.IsClosingActiveAsync(cancellationToken);
        if (isActive)
        {
            return Result.Failure(new Error("0001", "Ya existe un proceso de cierre activo.", ErrorType.Validation));
        }

        var now = DateTime.UtcNow;

        // Guardar en caché: lo hace el servicio directamente solo en el inicio
        await store.BeginAsync(now, cancellationToken);

        // Publicar evento de inicio
        await stepEventPublisher.PublishAsync(portfolioId, ClosingProcess.Begin.ToString(), now, now, cancellationToken);

        return Result.Success();
    }

    public async Task UpdateStepAsync(int portfolioId, string process, DateTime processDatetime, CancellationToken cancellationToken)
    {
        // Recupera closingDatetime original para mantenerlo constante en los eventos
        var closingDatetime = await store.GetClosingDatetimeAsync(cancellationToken);

        if (closingDatetime is null)
        {
            throw new InvalidOperationException("No se ha iniciado el proceso de cierre para el portafolio.");
        }

        await store.UpdateProcessAsync(process, processDatetime, cancellationToken);
        // Publicar evento de avance o fin de cierre
        await stepEventPublisher.PublishAsync(portfolioId, process, closingDatetime.Value, processDatetime, cancellationToken);
    }

    public async Task EndAsync(int portfolioId, CancellationToken cancellationToken)
    {
        // Recupera closingDatetime original
        var closingDatetime = await store.GetClosingDatetimeAsync(cancellationToken);
        var now = DateTime.UtcNow;
        if (closingDatetime is null)
        {
            throw new InvalidOperationException("No se ha iniciado el proceso de cierre para el portafolio.");
        }

        // Publicar evento final: el consumer se encargará de borrar la caché
        await stepEventPublisher.PublishAsync(portfolioId, ClosingProcess.End.ToString(), closingDatetime.Value, now, cancellationToken);
    }
}