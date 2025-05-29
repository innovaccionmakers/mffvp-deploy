using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using MediatR;
using Products.Integrations.ContributionValidation;

namespace Products.IntegrationEvents.ContributionValidation;

public sealed class ContributionValidationConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public ContributionValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(ContributionValidationRequest))]
    public async Task<ContributionValidationResponse> ValidateAsync(
        ContributionValidationRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result =
            await _mediator.Send(
                new ContributionValidationQuery(message.ActivateId, message.ObjectiveId, message.PortfolioStandardCode, message.DepositDate,
                    message.ExecutionDate, message.Amount), cancellationToken);

        return result.Match(
            ok => new ContributionValidationResponse(true),
            err => new ContributionValidationResponse(
                false,
                err.Error.Code,
                err.Error.Description));
    }
}