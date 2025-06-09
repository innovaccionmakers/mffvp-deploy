using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Products.IntegrationEvents.ContributionValidation;

namespace Operations.Infrastructure.External.ContributionValidation;

internal sealed class ContributionRemoteValidator(ICapRpcClient rpc) : IContributionRemoteValidator
{
    public async Task<Result<ContributionRemoteData>> ValidateAsync(
        int activateId,
        int objectiveId,
        string? portfolioId,
        DateTime deposit,
        DateTime exec,
        decimal amount,
        CancellationToken ct)
    {
        var rsp = await rpc.CallAsync<
            ContributionValidationRequest,
            ContributionValidationResponse>(
            nameof(ContributionValidationRequest),
            new ContributionValidationRequest(activateId, objectiveId, portfolioId, deposit, exec, amount),
            TimeSpan.FromSeconds(5),
            ct);

        return rsp.IsValid
            ? Result.Success(new ContributionRemoteData(
                rsp.AffiliateId!.Value,
                rsp.ObjectiveId!.Value,
                rsp.PortfolioId!.Value,
                rsp.PortfolioName!,
                rsp.PortfolioInitialMinimumAmount!.Value))
            : Result.Failure<ContributionRemoteData>(Error.Validation(rsp.Code, rsp.Message));
    }
}