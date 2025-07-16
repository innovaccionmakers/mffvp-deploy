using Closing.Application.Abstractions.Data;
using Closing.Domain.TrustYields;
using Closing.Integrations.TrustSync;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Closing.Application.TrustSync;

internal sealed class TrustSyncCommandHandler(
    ITrustYieldRepository repository,
    IUnitOfWork unitOfWork) : ICommandHandler<TrustSyncCommand, bool>
{
    public async Task<Result<bool>> Handle(TrustSyncCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var closingDateUtc = request.ClosingDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.ClosingDate, DateTimeKind.Utc)
            : request.ClosingDate.ToUniversalTime();
        var processDateUtc = DateTime.UtcNow;

        var existing = await repository.GetByTrustAndDateAsync(request.TrustId, closingDateUtc, cancellationToken);
        if (existing is null)
        {
            var result = TrustYield.Create(
                request.TrustId,
                request.PortfolioId,
                closingDateUtc,
                0m,
                0m,
                0m,
                request.PreClosingBalance,
                0m,
                0m,
                0m,
                0m,
                0m,
                request.Capital,
                processDateUtc,
                request.ContingentWithholding,
                0m);
            if (result.IsFailure)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result.Failure<bool>(result.Error!);
            }
            await repository.InsertAsync(result.Value, cancellationToken);
        }
        else
        {
            existing.UpdateDetails(
                existing.TrustId,
                existing.PortfolioId,
                closingDateUtc,
                existing.Participation,
                existing.Units,
                existing.YieldAmount,
                request.PreClosingBalance,
                existing.ClosingBalance,
                existing.Income,
                existing.Expenses,
                existing.Commissions,
                existing.Cost,
                request.Capital,
                processDateUtc,
                request.ContingentWithholding,
                existing.YieldRetention);
            repository.Update(existing);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success(true);
    }
}
