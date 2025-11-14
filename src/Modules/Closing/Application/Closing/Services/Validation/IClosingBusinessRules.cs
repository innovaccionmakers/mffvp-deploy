// Closing.Application.Closing.Services.Validation
using Common.SharedKernel.Domain;

namespace Closing.Application.Closing.Services.Validation;

public interface IClosingBusinessRules
{
    Task<Result<bool>> IsFirstClosingDayAsync(int portfolioId, CancellationToken cancellationToken);
    Task<Result<bool>> HasDebitNotesAsync(int portfolioId, DateTime closingDateUtc, CancellationToken cancellationToken);
}
