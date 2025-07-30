
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.AccumulatedCommissions;
using Products.Integrations.AccumulatedCommissions.Commands;

namespace Products.Application.AccumulatedCommissions.Commands;

internal sealed class UpsertAccumulatedCommissionCommandHandler :
    ICommandHandler<UpsertAccumulatedCommissionCommand>
{
    private readonly IAccumulatedCommissionRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpsertAccumulatedCommissionCommandHandler(
        IAccumulatedCommissionRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpsertAccumulatedCommissionCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        var existing = await _repository
            .GetByPortfolioAndCommissionAsync(
                request.PortfolioId,
                request.CommissionId,
                cancellationToken);

        if (existing is not null)
        {
            // Actualizar acumulado; los valores pagados y pendientes calculados
            var paid = existing.PaidValue;
            var pending = request.AccumulatedValue - paid;

            existing.UpdateValues(
                newAccumulatedValue: request.AccumulatedValue,
                newPaidValue: paid,
                newPendingValue: pending,
                newCloseDate: request.CloseDate,
                newPaymentDate: existing.PaymentDate,
                newProcessDate: DateTime.UtcNow
            );
        }
        else
        {
            // Crear nuevo registro
            var pending = request.AccumulatedValue;

            var createResult = AccumulatedCommission.Create(
                portfolioId: request.PortfolioId,
                commissionId: request.CommissionId,
                accumulatedValue: request.AccumulatedValue,
                paidValue: 0m,
                pendingValue: pending,
                closeDate: request.CloseDate,
                paymentDate: default,
                processDate: DateTime.UtcNow
            );

            if (createResult.IsFailure)
                return Result.Failure(createResult.Error);

            await _repository.AddAsync(createResult.Value, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }
}