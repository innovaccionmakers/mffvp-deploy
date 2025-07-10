using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Abstractions.Services.QueueTransactions;

public interface IQueueTransactions
{
    Task<Result<ContributionResponse>> ExecuteAsync(
        CreateContributionCommand command,
        PrevalidationResult prevalidationResult,
        CancellationToken cancellationToken);
}
