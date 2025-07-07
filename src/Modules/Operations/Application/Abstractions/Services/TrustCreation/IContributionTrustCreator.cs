using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Abstractions.Services.TrustCreation;

public interface IContributionTrustCreator
{
    Task ExecuteAsync(
        CreateContributionCommand command,
        ClientOperation clientOperation,
        TaxResult taxResult,
        CancellationToken cancellationToken);
} 