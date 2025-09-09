using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain; 
using Microsoft.Extensions.Logging;
using Npgsql;
using Products.Application.Abstractions.Data;
using Products.Domain.AccumulatedCommissions;
using Products.Integrations.AccumulatedCommissions.Commands;

namespace Products.Application.AccumulatedCommissions.Commands;

internal sealed class UpsertAccumulatedCommissionCommandHandler :
    ICommandHandler<UpsertAccumulatedCommissionCommand>
{
    private const string ClassName = nameof(UpsertAccumulatedCommissionCommandHandler);

    private readonly IAccumulatedCommissionRepository repository;
    private readonly IUnitOfWork unitOfWork;
    private readonly ILogger<UpsertAccumulatedCommissionCommandHandler> logger;

    public UpsertAccumulatedCommissionCommandHandler(
        IAccumulatedCommissionRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<UpsertAccumulatedCommissionCommandHandler> logger)
    {
        this.repository = repository;
        this.unitOfWork = unitOfWork;
        this.logger = logger;
    }

    public async Task<Result> Handle(
        UpsertAccumulatedCommissionCommand request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "{Class} - Inicio manejo. Portafolio:{PortfolioId} Comisión:{CommissionId} FechaCierreEvento:{CloseDate:yyyy-MM-dd} MontoDiarioEvento:{DailyAmount}",
            ClassName, request.PortfolioId, request.CommissionId, request.CloseDate, request.AccumulatedValue);

        await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            // UPSERT SQL-first (diferencia del día); idempotente por fecha de cierre
            var aplicado = await repository.UpsertAsync(
                portfolioId: request.PortfolioId,
                commissionId: request.CommissionId,
                closingDateEventUtc: request.CloseDate,
                dailyAmount: request.AccumulatedValue,
                cancellationToken: cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var snap = await repository.GetByPortfolioAndCommissionAsync(
                request.PortfolioId, request.CommissionId, cancellationToken);

            if (snap is not null)
            {
                logger.LogInformation(
                    "{Class} - Fin manejo ({Outcome}). Portafolio:{PortfolioId} Comisión:{CommissionId} FechaCierreAlmacenada:{CloseStored:yyyy-MM-dd} Acumulado:{Accumulated} Pagado:{Paid} Pendiente:{Pending} | FechaCierreEvento:{CloseEvent:yyyy-MM-dd} MontoDiarioEvento:{DailyAmount}",
                    ClassName,
                    aplicado ? "aplicado" : "sin cambios",
                    request.PortfolioId,
                    request.CommissionId,
                    snap.CloseDate,
                    snap.AccumulatedValue,
                    snap.PaidValue,
                    snap.PendingValue,
                    request.CloseDate,
                    request.AccumulatedValue
                );
            }
            else
            {
                logger.LogWarning(
                    "{Class} - No se encontró snapshot después del upsert. Portafolio:{PortfolioId} Comisión:{CommissionId}",
                    ClassName, request.PortfolioId, request.CommissionId);
            }

            return Result.Success();
        }
        catch (PostgresException ex)
        {
            await tx.RollbackAsync(CancellationToken.None);
            logger.LogError(ex,
                "{Class} - Error de base de datos (SQLSTATE {SqlState}). Portafolio:{PortfolioId} Comisión:{CommissionId}",
                ClassName, ex.SqlState, request.PortfolioId, request.CommissionId);

            // Código y tipo de error con estructura solicitada
            return Result.Failure(new Error(
                "PROD-COM-001",
                $"Error de base de datos (SQLSTATE {ex.SqlState}) al actualizar comisiones acumuladas para el portafolio {request.PortfolioId} y la comisión {request.CommissionId}.",
                ErrorType.Failure));
        }
        catch (OperationCanceledException)
        {
            await tx.RollbackAsync(CancellationToken.None);
            logger.LogWarning(
                "{Class} - Operación cancelada. Portafolio:{PortfolioId} Comisión:{CommissionId}",
                ClassName, request.PortfolioId, request.CommissionId);
            throw;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(CancellationToken.None);
            logger.LogError(ex,
                "{Class} - Error inesperado. Portafolio:{PortfolioId} Comisión:{CommissionId}",
                ClassName, request.PortfolioId, request.CommissionId);

            return Result.Failure(new Error(
                "PROD-COM-999",
                $"Error inesperado al actualizar comisiones acumuladas para el portafolio {request.PortfolioId} y la comisión {request.CommissionId}.",
                ErrorType.Failure));
        }
    }
}
