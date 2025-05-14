using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.TrustHistories;
using Trusts.Domain.Trusts;
using Trusts.Integrations.TrustHistories;
using Trusts.Integrations.TrustHistories.CreateTrustHistory;

namespace Trusts.Application.TrustHistories.CreateTrustHistory;

internal sealed class CreateTrustHistoryCommandHandler(
    ITrustRepository trustRepository,
    ITrustHistoryRepository trusthistoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTrustHistoryCommand, TrustHistoryResponse>
{
    public async Task<Result<TrustHistoryResponse>> Handle(CreateTrustHistoryCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);

        if (trust is null)
            return Result.Failure<TrustHistoryResponse>(TrustErrors.NotFound(request.TrustId));


        var result = TrustHistory.Create(
            request.Earnings,
            request.Date,
            request.SalesUserId,
            trust
        );

        if (result.IsFailure) return Result.Failure<TrustHistoryResponse>(result.Error);

        var trusthistory = result.Value;

        trusthistoryRepository.Insert(trusthistory);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustHistoryResponse(
            trusthistory.TrustHistoryId,
            trusthistory.TrustId,
            trusthistory.Earnings,
            trusthistory.Date,
            trusthistory.SalesUserId
        );
    }
}