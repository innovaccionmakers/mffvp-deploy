using Common.SharedKernel.Application.EventBus;
using DotNetCore.CAP;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.Application.Abstractions.Data;
using Operations.Domain.OperationTypes;
using Operations.Domain.TrustOperations;
using Operations.IntegrationEvents.TrustOperations;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.Application.TrustOperations.Commands;

internal sealed class UpsertTrustOperationCommandHandler(
    ITrustOperationRepository repository,
    IUnitOfWork unitOfWork,
    IOperationTypeRepository operationTypeRepository,
    IEventBus eventBus)
    : IRequestHandler<UpsertTrustOperationCommand>
{
    private const string ClassName = nameof(UpsertTrustOperationCommandHandler);
    public async Task Handle(UpsertTrustOperationCommand request, CancellationToken cancellationToken)
    {
        

        // 1. Obtener el subtipo "Rendimientos"
        var subtype = await operationTypeRepository
            .GetByNameAsync("Rendimientos", cancellationToken);
        if (subtype is null)
        {
            throw new InvalidOperationException("Tipo de Transacción 'Rendimientos' no encontrada.");
        }

        var yieldSubtypeId = subtype.OperationTypeId;

        //Definicion: para la tabla operaciones.operaciones_fideicomiso, la fecha de radicación y aplicación es el getdate
        //y la fecha de proceso es la fecha del último cierre. 

        using (var connection = unitOfWork.GetDbConnection())
        
        using (var transaction = connection.BeginTransaction(eventBus.GetCapPublisher(), autoCommit: false))
        { 
                // Upsert ExecuteSqlInterpolatedAsync
            bool changed = await repository.UpsertAsync(
            portfolioId: request.PortfolioId,
            trustId: request.TrustId,
            processDate: request.ClosingDate,
            amount: request.Amount,
            operationTypeId: yieldSubtypeId,
            clientOperationId: null,
            cancellationToken: cancellationToken);

            if (changed)
            {

                    var integrationEvent = new TrustYieldOperationAppliedIntegrationEvent(
                        trustId: request.TrustId,
                        portfolioId: request.PortfolioId,
                        closingDate: request.ClosingDate,
                        yieldAmount: request.Amount,
                        yieldRetention: request.YieldRetention,
                        closingBalance: request.ClosingBalance
                    );

                    await eventBus.PublishAsync(integrationEvent, cancellationToken);
                }

         await transaction.CommitAsync(cancellationToken);
        }
    }
}
