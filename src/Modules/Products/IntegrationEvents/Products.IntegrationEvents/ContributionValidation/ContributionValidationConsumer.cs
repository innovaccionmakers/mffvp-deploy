using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Products.Integrations.ContributionValidation;

namespace Products.IntegrationEvents.ContributionValidation;

public sealed class ContributionValidationConsumer : IRpcHandler<ContributionValidationRequest, ContributionValidationResponse>
{
    private readonly ISender _mediator;

    public ContributionValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<ContributionValidationResponse> HandleAsync(
        ContributionValidationRequest message,
        CancellationToken cancellationToken)
    {
        var result =
            await _mediator.Send(
                new ContributionValidationQuery(message.ActivateId, message.ObjectiveId,
                    message.PortfolioHomologatedCode,
                    message.DepositDate,
                    message.ExecutionDate, message.Amount), cancellationToken);

        return result.Match(
            ok => new ContributionValidationResponse(
                true,
                AffiliateId: result.Value.AffiliateId,
                ObjectiveId: result.Value.ObjectiveId,
                PortfolioId: result.Value.PortfolioId,
                PortfolioInitialMinimumAmount: result.Value.PortfolioInitialMinimumAmount,
                PortfolioAdditionalMinimumAmount: result.Value.PortfolioAdditionalMinimumAmount,
                PortfolioName: result.Value.PortfolioName
                ),
            err => new ContributionValidationResponse(
                false,
                err.Error.Code,
                err.Error.Description));
    }
}