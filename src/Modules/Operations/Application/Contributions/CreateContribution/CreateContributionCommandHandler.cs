using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Application.Abstractions.Services.QueueTransactions;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Abstractions.Services.TrustCreation;
using Operations.Integrations.Contributions.CreateContribution;
using Operations.Integrations.Contributions;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IPrevalidate prevalidator,
    IClosingValidator closingValidator,
    IQueueTransactions queueTransactions,
    ITransactionControl transactionControl,
    ITrustCreation trustCreation)
    : ICommandHandler<CreateContributionCommand, ContributionResponse>
{
    public async Task<Result<ContributionResponse>> Handle(CreateContributionCommand command,
        CancellationToken cancellationToken)
    {
        var prevalidationResult = await prevalidator.ValidateAsync(command, cancellationToken);

        if (!prevalidationResult.IsSuccess)
            return Result.Failure<ContributionResponse>(prevalidationResult.Error!);

        var portfolioId = prevalidationResult.Value.RemoteData.PortfolioId;
        if (await closingValidator.IsClosingActiveAsync(portfolioId, cancellationToken))
        {
            return await queueTransactions.ExecuteAsync(command, prevalidationResult.Value, cancellationToken);
        }

        var (operation, taxResult) = await transactionControl.ExecuteAsync(command, prevalidationResult.Value, cancellationToken);

        await trustCreation.ExecuteAsync(command, operation, taxResult, cancellationToken);

        var response = new ContributionResponse(
            operation.ClientOperationId,
            operation.PortfolioId.ToString(),
            prevalidationResult.Value.RemoteData.PortfolioName,
            taxResult.TaxConditionName,
            taxResult.WithheldAmount);

        return Result.Success(response, "¡Genial! El aporte ha sido procesado exitosamente y ya se encuentra registrado en el portafolio.");
    }
}
