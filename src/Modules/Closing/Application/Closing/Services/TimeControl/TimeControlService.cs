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
    public async Task<Result> StartAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        // Validación: ¿hay cierre activo?
        var isActive = await store.IsClosingActiveAsync(portfolioId, cancellationToken);
        if (isActive)
        {
            return Result.Failure(new Error("0001", "Ya existe un proceso de cierre activo para el portafolio.", ErrorType.Validation));
        }

        var now = DateTime.UtcNow;

        // Guardar en caché: lo hace el servicio directamente solo en el inicio
        await store.BeginAsync(portfolioId, closingDate, cancellationToken);

        // Publicar evento de inicio
        await stepEventPublisher.PublishAsync(portfolioId, ClosingProcess.Begin.ToString(), closingDate, cancellationToken);

        return Result.Success();
    }

    public async Task UpdateStepAsync(int portfolioId, string process, DateTime processDatetime, CancellationToken cancellationToken)
    {
        // Recupera closingDatetime original para mantenerlo constante en los eventos
        var closingDatetime = await store.GetClosingDatetimeAsync(portfolioId, cancellationToken);

        if (closingDatetime is null)
        {
            throw new InvalidOperationException("No se ha iniciado el proceso de cierre para este portafolio.");
        }

        // Publicar evento de avance o fin de cierre
        await stepEventPublisher.PublishAsync(portfolioId, process, closingDatetime.Value, cancellationToken);
    }

    public async Task EndAsync(int portfolioId, CancellationToken cancellationToken)
    {
        // Recupera closingDatetime original
        var closingDatetime = await store.GetClosingDatetimeAsync(portfolioId, cancellationToken);

        if (closingDatetime is null)
        {
            throw new InvalidOperationException("No se ha iniciado el proceso de cierre para este portafolio.");
        }

        // Publicar evento final: el consumer se encargará de borrar la caché
        await stepEventPublisher.PublishAsync(portfolioId, ClosingProcess.End.ToString(), closingDatetime.Value, cancellationToken);
    }
}