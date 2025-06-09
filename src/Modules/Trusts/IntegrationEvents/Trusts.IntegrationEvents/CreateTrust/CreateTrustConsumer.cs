using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using MediatR;
using Trusts.Integrations.Trusts.CreateTrust;
using Trusts.Integrations.Trusts;

namespace Trusts.IntegrationEvents.CreateTrust;

public sealed class CreateTrustConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public CreateTrustConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(CreateTrustRequest))]
    public async Task<CreateTrustResponse> Handle(
        CreateTrustRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result = await _mediator.Send(new CreateTrustCommand(
                message.AffiliateId,
                message.ClientOperationId,
                message.CreationDate,
                message.ObjectiveId,
                message.PortfolioId,
                message.TotalBalance,
                message.TotalUnits,
                message.Principal,
                message.Earnings,
                message.TaxCondition,
                message.ContingentWithholding,
                message.EarningsWithholding,
                message.AvailableAmount),
            cancellationToken);

        return result.Match(
            ok => new CreateTrustResponse(true, ok, null, null),
            err => new CreateTrustResponse(false, null, err.Error.Code, err.Error.Description));
    }
}