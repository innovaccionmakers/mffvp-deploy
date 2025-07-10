using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Abstractions.Services.Closing;

public interface IClosingValidator
{
    Task<bool> IsClosingActiveAsync(int portfolioId, CancellationToken cancellationToken);
}