using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Domain.TrustYields;
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.TrustSync;

public sealed class TrustSyncService : IDataSyncService
{
    private readonly ITrustLocator _trustLocator;
    private readonly ITrustYieldRepository _yieldTrustRepository;

    public TrustSyncService(
        ITrustLocator trustLocator,
        ITrustYieldRepository yieldTrustRepository
        )
    {
        _trustLocator = trustLocator;
        _yieldTrustRepository = yieldTrustRepository;
    }

    public async Task<Result> ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var result = await _trustLocator.GetActiveTrustsAsync(portfolioId, cancellationToken);

        if (result.IsFailure)
            return Result.Failure(result.Error);

        foreach (var trust in result.Value)
        {
            var snapshot = new YieldTrustSnapshot
            {
                TrustId = trust.TrustId,
                PortfolioId = trust.PortfolioId,
                ClosingDate = closingDate,
                PreClosingBalance = trust.TotalBalance,
                Capital = trust.Principal,
                ContingentRetention = trust.ContingentWithholding,
                ProcessDate = DateTime.UtcNow,
            };

            await _yieldTrustRepository.UpsertAsync(snapshot, cancellationToken);
        }

        return Result.Success();
    }
}