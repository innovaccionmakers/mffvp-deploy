using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Operations.Application.Abstractions.Services.Closing;

namespace Operations.Application.Contributions.Services.ClosingValidator;

public sealed class ClosingValidator(
    IClosingExecutionStore closingStore) : IClosingValidator
{
    public Task<bool> IsClosingActiveAsync(int portfolioId, CancellationToken cancellationToken) =>
        closingStore.IsClosingActiveAsync(portfolioId, cancellationToken);
}
