using Common.SharedKernel.Application.EventBus;
using MediatR;
using Operations.Application.Abstractions.Data;
using Operations.Domain.SubtransactionTypes;
using Operations.Domain.TrustOperations;
using Operations.IntegrationEvents.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.Application.TrustOperations.Commands;

internal sealed class UpsertTrustOperationCommandHandler(
    ITrustOperationRepository repository,
    IUnitOfWork unitOfWork,
    ISubtransactionTypeRepository subtransactionTypeRepository,
    IEventBus eventBus)
    : IRequestHandler<UpsertTrustOperationCommand>
{
    public async Task Handle(UpsertTrustOperationCommand request, CancellationToken cancellationToken)
    {
        // 1. Obtener el subtipo "Rendimientos"
        var subtype = await subtransactionTypeRepository
            .GetByNameAsync("Rendimientos", cancellationToken);
        if (subtype is null)
            throw new InvalidOperationException("Tipo de Transacción 'Rendimientos' no encontrada.");

        var yieldSubtypeId = subtype.SubtransactionTypeId;

        // 2. Intentar cargar una operación de fideicomiso existente para este portafolio y fecha de cierre
        var existing = await repository
            .GetByPortfolioAndTrustAsync(request.PortfolioId, request.TrustId, request.ClosingDate, cancellationToken);

        if (existing is not null)
        {
            // 3a. Actualizar la operación existente
            existing.UpdateDetails(
                newClientOperationId: null,
                newTrustId: request.TrustId,
                newAmount: request.Amount,
                newSubtransactionTypeId: yieldSubtypeId,
                newPortfolioId: request.PortfolioId,
                newRegistrationDate: request.ProcessDate,
                newProcessDate: request.ClosingDate,
                newApplicationDate: request.ProcessDate
            );

            repository.Update(existing); 
        }
        else
        {
            // 3b. Crear una nueva operación
            var opResult = TrustOperation.Create(
                clientOperationId: null,
                trustId: request.TrustId,
                amount: request.Amount,
                subtransactionTypeId: yieldSubtypeId,
                portfolioId: request.PortfolioId,
                registrationDate: request.ProcessDate,
                processDate: request.ClosingDate,
                applicationDate: request.ProcessDate
            );

            if (opResult.IsFailure)
                throw new InvalidOperationException(
                    $"Error creando operación de fideicomiso: {opResult.Error.Description}"
                );

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
            closingBalance: request.ClosingBalance,
            units: request.Units
        );

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
