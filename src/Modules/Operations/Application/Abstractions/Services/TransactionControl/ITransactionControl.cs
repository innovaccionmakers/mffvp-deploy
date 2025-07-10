using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Contributions.Prevalidation;
using Operations.Domain.ClientOperations;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Abstractions.Services.TransactionControl;

public interface ITransactionControl
{
    Task<(ClientOperation Operation, TaxResult Tax)> ExecuteAsync(
        CreateContributionCommand command,
        PrevalidationResult prevalidationResult,
        CancellationToken cancellationToken);
} 