using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using DataSync.Integrations.TrustSync;

namespace DataSync.Application.TrustSync;

internal sealed class TrustSyncCommandHandler(
    ITrustDataService trustDataService,
    IYieldSyncService yieldSyncService) : ICommandHandler<TrustSyncCommand, bool>
{
    public async Task<Result<bool>> Handle(TrustSyncCommand request, CancellationToken cancellationToken)
    {
        var trustsResult = await trustDataService.GetActiveTrustsByPortfolioAsync(
            request.PortfolioId, 
            cancellationToken);

        if (trustsResult.IsFailure)
            return Result.Failure<bool>(trustsResult.Error);

        var closingDateUtc = request.ClosingDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.ClosingDate, DateTimeKind.Utc)
            : request.ClosingDate.ToUniversalTime();

        foreach (var trust in trustsResult.Value)
        {
            var syncResult = await yieldSyncService.SyncTrustYieldAsync(
                trust.TrustId,
                trust.PortfolioId,
                closingDateUtc,
                trust.TotalBalance,
                trust.Principal,
                trust.ContingentWithholding,
                cancellationToken);

            if (syncResult.IsFailure)
                return Result.Failure<bool>(syncResult.Error);
        }

        return Result.Success(true);
    }
}
