using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Presentation.Results;
using DotNetCore.CAP;
using MediatR;
using Products.Integrations.Portfolios.GetPortfolio;

namespace Products.IntegrationEvents.PortfolioValidation;

public sealed class PortfolioValidationConsumer : ICapSubscribe
{
    private readonly ISender _mediator;

    public PortfolioValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    [CapSubscribe(nameof(ValidatePortfolioRequest))]
    public async Task<ValidatePortfolioResponse> ValidateAsync(
        ValidatePortfolioRequest message,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
    {
        var corr = header[CapRpcClient.Headers.CorrelationId];
        header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, corr);

        var result = await _mediator.Send(new GetPortfolioQuery(message.PortfolioId), cancellationToken);

        return result.Match(
            _ => new ValidatePortfolioResponse(true, null, null),
            err => new ValidatePortfolioResponse(false, err.Error.Code, err.Error.Description));
    }
}