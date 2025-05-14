using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.TrustHistories;
using Trusts.Integrations.TrustHistories;
using Trusts.Integrations.TrustHistories.UpdateTrustHistory;

namespace Trusts.Application.TrustHistories;

internal sealed class UpdateTrustHistoryCommandHandler(
    ITrustHistoryRepository trusthistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTrustHistoryCommand, TrustHistoryResponse>
{
    public async Task<Result<TrustHistoryResponse>> Handle(UpdateTrustHistoryCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await trusthistoryRepository.GetAsync(request.TrustHistoryId, cancellationToken);
        if (entity is null)
            return Result.Failure<TrustHistoryResponse>(TrustHistoryErrors.NotFound(request.TrustHistoryId));

        entity.UpdateDetails(
            request.NewTrustId,
            request.NewEarnings,
            request.NewDate,
            request.NewSalesUserId
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustHistoryResponse(entity.TrustHistoryId, entity.TrustId, entity.Earnings, entity.Date,
            entity.SalesUserId);
    }
}