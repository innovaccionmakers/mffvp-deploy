using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Abstractions.Services.TrustCreation;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;

namespace Operations.Application.Contributions.CreateContribution;

internal sealed class CreateContributionCommandHandler(
    IContributionPrevalidator prevalidator,
    IContributionTransactionControl transactionControl,
    IContributionTrustCreator trustCreator)
    : ICommandHandler<CreateContributionCommand, ContributionResponse>
{
    public async Task<Result<ContributionResponse>> Handle(CreateContributionCommand command,
        CancellationToken cancellationToken)
    {
        var prevalidationResult = await prevalidator.ValidateAsync(command, cancellationToken);

        if (!prevalidationResult.IsSuccess)
            return Result.Failure<ContributionResponse>(prevalidationResult.Error!);

        var (operation, tax) = await transactionControl.ExecuteAsync(command, prevalidationResult.Value, cancellationToken);

        await trustCreator.ExecuteAsync(command, operation, tax, cancellationToken);

        var resp = new ContributionResponse(
            operation.ClientOperationId,
            operation.PortfolioId.ToString(),
            prevalidationResult.Value.RemoteData.PortfolioName,
            tax.TaxConditionName,
            tax.WithheldAmount);

        return Result.Success(resp, "Transacción causada Exitosamente");
    }
}