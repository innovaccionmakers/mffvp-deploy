using Closing.Application.PreClosing.Services.ExtraReturns.Dto;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.ExtraReturns.Interfaces;

public interface IExtraReturnConsolidationService
{
    Task<Result<IReadOnlyCollection<ExtraReturnSummary>>> GetExtraReturnSummariesAsync(
        int portfolioId,
        DateTime closingDateUtc,
        CancellationToken cancellationToken);
}
