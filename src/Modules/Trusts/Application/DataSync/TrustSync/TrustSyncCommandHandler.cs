using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Domain.Trusts;
using Trusts.Application.Abstractions.External;
using Trusts.Integrations.DataSync.TrustSync;

namespace Trusts.Application.DataSync.TrustSync;

internal sealed class TrustSyncCommandHandler(
    ITrustRepository trustRepository,
    ITrustYieldSyncService syncService) : ICommandHandler<TrustSyncCommand, bool>
{
    public async Task<Result<bool>> Handle(TrustSyncCommand request, CancellationToken cancellationToken)
    {
        var trusts = await trustRepository.GetAllAsync(cancellationToken);
        var activeTrusts = trusts.Where(t => t.Status).ToArray();

        var closingDateUtc = request.ClosingDate.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(request.ClosingDate, DateTimeKind.Utc)
            : request.ClosingDate.ToUniversalTime();

        foreach (var trust in activeTrusts)
        {
            var result = await syncService.SyncAsync(
                (int)trust.TrustId,
                trust.PortfolioId,
                closingDateUtc,
                trust.TotalBalance,
                trust.Principal,
                trust.ContingentWithholding,
                cancellationToken);

            if (result.IsFailure)
                return Result.Failure<bool>(result.Error!);
        }

        return Result.Success(true);
    }
}

