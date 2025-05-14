using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.TrustHistories;
using Trusts.Integrations.TrustHistories.DeleteTrustHistory;

namespace Trusts.Application.TrustHistories.DeleteTrustHistory;

internal sealed class DeleteTrustHistoryCommandHandler(
    ITrustHistoryRepository trusthistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTrustHistoryCommand>
{
    public async Task<Result> Handle(DeleteTrustHistoryCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var trusthistory = await trusthistoryRepository.GetAsync(request.TrustHistoryId, cancellationToken);
        if (trusthistory is null) return Result.Failure(TrustHistoryErrors.NotFound(request.TrustHistoryId));

        trusthistoryRepository.Delete(trusthistory);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}