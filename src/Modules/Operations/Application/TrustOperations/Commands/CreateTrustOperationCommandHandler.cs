using Common.SharedKernel.Application.EventBus;
using MediatR;
using Operations.Application.Abstractions.Data;
using Operations.Domain.SubtransactionTypes;
using Operations.Domain.TrustOperations;
using Operations.IntegrationEvents.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.Application.TrustOperations.Commands;

internal sealed class CreateTrustOperationCommandHandler(
    ITrustOperationRepository repository,
    IUnitOfWork unitOfWork,
    ISubtransactionTypeRepository subtransactionTypeRepository,
    IEventBus eventBus)
    : IRequestHandler<CreateTrustOperationCommand>
{
    public async Task Handle(CreateTrustOperationCommand request, CancellationToken cancellationToken)
    {
        // TODO: Reemplazar por consulta a tipo de transacción "Rendimientos".
        const long yieldSubtypeId = 4;

                /*
           var subtype = await subtransactionTypeRepository.GetByNameAsync("Rendimientos", cancellationToken);
           if (subtype is null)
           {
               throw new InvalidOperationException("Subtransaction type 'Rendimientos' not found.");
           }
           var rendimentoSubtypeId = subtype.SubtransactionTypeId;
           */

        var operation = TrustOperation.Create(
            clientOperationId: 0, // No relacionada con cliente
            trustId: request.TrustId,
            amount: request.Amount,
            subtransactionTypeId: yieldSubtypeId,
            portfolioId: request.PortfolioId,
            registrationDate: request.ClosingDate,
            processDate: request.ProcessDate,
            applicationDate: request.ClosingDate
        );

        if (operation.IsFailure)
        {
            throw new InvalidOperationException($"Error creando operacion fideicomiso: {operation.Error.Description}");
        }

        await repository.AddAsync(operation.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var integrationEvent = new TrustYieldOperationAppliedIntegrationEvent(
            trustId: request.TrustId,
            portfolioId: request.PortfolioId,
            closingDate: request.ClosingDate,
            yieldAmount: request.Amount,
            yieldRetention: 0m, // TODO: ajustar para enviar la retención real
            closingBalance: 0m  // TODO: ajustar para validar contra valor esperado en Trust
        );

        await eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}