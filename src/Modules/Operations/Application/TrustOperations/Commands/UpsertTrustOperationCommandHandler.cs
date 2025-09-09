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

        logger.LogInformation("{Class} - Iniciando Upsert con SQL Crudo de operación de fideicomiso {fideicomisoId}.",ClassName, request.TrustId);

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
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        // Upsert ExecuteSqlInterpolatedAsync
        bool changed = await repository.UpsertAsync(
            portfolioId: request.PortfolioId,
            trustId: request.TrustId,
            processDate: request.ClosingDate,
            amount: request.Amount,
            operationTypeId: yieldSubtypeId,
            clientOperationId: null,
            cancellationToken: cancellationToken);
        logger.LogInformation("{Class} -  Upsert ejecutado para operación de fideicomiso {fideicomisoId}, fecha cierre: {fechaCierre}.", ClassName, request.TrustId, request.ClosingDate);
        if (changed)
        {
            logger.LogInformation("{Class} -  Hubo cambios en Upsert de operación de fideicomiso {fideicomisoId}," +
                " fecha cierre: {fechaCierre}. Se dispara evento de actualizacion de Fideicomiso", ClassName, request.TrustId, request.ClosingDate);
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
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
