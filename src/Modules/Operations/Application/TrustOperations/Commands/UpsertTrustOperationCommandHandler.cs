using Common.SharedKernel.Application.EventBus;
using MediatR;
using Operations.Application.Abstractions.Data;
using Operations.Domain.OperationTypes;
using Operations.Domain.TrustOperations;
using Operations.IntegrationEvents.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;
using Microsoft.Extensions.Logging;

namespace Operations.Application.TrustOperations.Commands;

internal sealed class UpsertTrustOperationCommandHandler(
    ITrustOperationRepository repository,
    IUnitOfWork unitOfWork,
    IOperationTypeRepository operationTypeRepository,
    IEventBus eventBus,
    ILogger<UpsertTrustOperationCommandHandler> logger)
    : IRequestHandler<UpsertTrustOperationCommand>
{
    private const string ClassName = nameof(UpsertTrustOperationCommandHandler);
    public async Task Handle(UpsertTrustOperationCommand request, CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["PortfolioId"] = request.PortfolioId,
            ["TrustId"] = request.TrustId,
            ["ClosingDate"] = request.ClosingDate.Date
        });

        logger.LogInformation("{Class} - Iniciando Upsert de operación de fideicomiso.",ClassName);

        // 1. Obtener el subtipo "Rendimientos"
        var subtype = await operationTypeRepository
            .GetByNameAsync("Rendimientos", cancellationToken);
        if (subtype is null)
        {
            throw new InvalidOperationException("Tipo de Transacción 'Rendimientos' no encontrada.");
        }
        //logger.LogInformation("SubtransactionType encontrado: Id={SubtypeId}", subtype.OperationTypeId);

        var yieldSubtypeId = subtype.OperationTypeId;

        // 2. Intentar cargar una operación de fideicomiso existente para el fideicomiso y fecha de cierre

        //Definicion: para la tabla operaciones.operaciones_fideicomiso, la fecha de radicación y aplicación es el getdate
        //y la fecha de proceso es la fecha del último cierre. 
       // logger.LogInformation("Consultando operación existente por (PortfolioId, TrustId, ClosingDate)...");
        var existing = await repository
            .GetForUpdateByPortfolioTrustAndDateAsync(request.PortfolioId, request.TrustId, request.ClosingDate.Date, cancellationToken);

        if (existing is not null)
        {
            //logger.LogInformation("Operación existente encontrada. Se procederá a actualizar (Id={OperationId}).", existing.TrustOperationId);

            // 3a. Actualizar la operación existente
            existing.UpdateDetails(
                newClientOperationId: null,
                newTrustId: request.TrustId,
                newAmount: request.Amount,
                newOperationTypeId: yieldSubtypeId,
                newPortfolioId: request.PortfolioId,
                newRegistrationDate: request.ProcessDate,
                newProcessDate: request.ClosingDate,
                newApplicationDate: request.ProcessDate
            );

            logger.LogInformation("Detalles actualizados: Amount={Amount}, SubtypeId={SubtypeId}, RegistrationDate={RegistrationDate}, ProcessDate={ProcessDate}, ApplicationDate={ApplicationDate}",
                request.Amount, yieldSubtypeId, request.ProcessDate, request.ClosingDate, request.ProcessDate);

            repository.Update(existing);
            logger.LogInformation("Repository.Update ejecutado para la operación existente.");
        }
        else
        {
            //logger.LogInformation("No existe operación previa. Se procederá a crear una nueva.");

            // 3b. Crear una nueva operación
            var opResult = TrustOperation.Create(
                clientOperationId: null,
                trustId: request.TrustId,
                amount: request.Amount,
                operationTypeId: yieldSubtypeId,
                portfolioId: request.PortfolioId,
                registrationDate: request.ProcessDate,
                processDate: request.ClosingDate,
                applicationDate: request.ProcessDate
            );

            if (opResult.IsFailure)
            {
                logger.LogError("Error creando operación de fideicomiso: {ErrorCode} {ErrorDescription}",
                    opResult.Error.Code, opResult.Error.Description);
                throw new InvalidOperationException(
                    $"Error creando operación de fideicomiso: {opResult.Error.Description}"
                );
            }

            await repository.AddAsync(opResult.Value, cancellationToken);
         
        }

        // 4. Guardar los cambios (insertar o actualizar) en una sola transacción
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 5. Publicar el evento de integración
        var integrationEvent = new TrustYieldOperationAppliedIntegrationEvent(
            trustId: request.TrustId,
            portfolioId: request.PortfolioId,
            closingDate: request.ClosingDate,
            yieldAmount: request.Amount,
            yieldRetention: request.YieldRetention,
            closingBalance: request.ClosingBalance
        );

        logger.LogInformation("Publicando evento TrustYieldOperationAppliedIntegrationEvent...");
        logger.LogInformation("Evento: {@IntegrationEvent}", integrationEvent);
        await eventBus.PublishAsync(integrationEvent, cancellationToken);
        logger.LogInformation("Evento publicado correctamente.");
    }
}
