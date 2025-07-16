using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.External;
using Products.IntegrationEvents.ContributionValidation;

namespace Operations.Infrastructure.External.ContributionValidation;

internal sealed class ContributionRemoteValidator(IRpcClient rpc) : IContributionRemoteValidator
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
            new ContributionValidationRequest(activateId, objectiveId, portfolioId, deposit, exec, amount),
            ct);

        return rsp.IsValid
            ? Result.Success(new ContributionRemoteData(
                rsp.AffiliateId!.Value,
                rsp.ObjectiveId!.Value,
                rsp.PortfolioId!.Value,
                rsp.PortfolioName!,
                rsp.PortfolioInitialMinimumAmount!.Value,
                rsp.PortfolioAdditionalMinimumAmount!.Value))
            : Result.Failure<ContributionRemoteData>(Error.Validation(rsp.Code, rsp.Message));
    }
}