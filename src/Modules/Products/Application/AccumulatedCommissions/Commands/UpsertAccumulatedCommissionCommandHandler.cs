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
            "{Class} - Portafolio:{PortfolioId} Comisión:{CommissionId} FechaCierreEvento:{CloseDate:yyyy-MM-dd} MontoDiarioEvento:{DailyAmount}",
            ClassName, request.PortfolioId, request.CommissionId, request.CloseDate, request.AccumulatedValue);

        try
        {
            var aplicado = await repository.UpsertAsync(
                portfolioId: request.PortfolioId,
                commissionId: request.CommissionId,
                closingDateEventUtc: request.CloseDate,
                dailyAmount: request.AccumulatedValue,
                cancellationToken: cancellationToken);

            return Result.Success();
        }
        catch (PostgresException ex)
        {
            logger.LogError(ex,
                "{Class} - Error de base de datos (SQLSTATE {SqlState}). Portafolio:{PortfolioId} Comisión:{CommissionId}",
                ClassName, ex.SqlState, request.PortfolioId, request.CommissionId);

            return Result.Failure(new Error(
                "PROD-COM-001",
                $"Error de base de datos (SQLSTATE {ex.SqlState}) al actualizar comisiones acumuladas para el portafolio {request.PortfolioId} y la comisión {request.CommissionId}.",
                ErrorType.Failure));
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "{Class} - Operación cancelada. Portafolio:{PortfolioId} Comisión:{CommissionId}",
                ClassName, request.PortfolioId, request.CommissionId);
            throw;
        }
        catch (Exception ex)
        {
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
