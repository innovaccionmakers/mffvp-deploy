using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Products.Integrations.Portfolios.GetPortfolio;

namespace Products.IntegrationEvents.PortfolioValidation;

public sealed class PortfolioValidationConsumer :
    IRpcHandler<ValidatePortfolioRequest, ValidatePortfolioResponse>,
    IRpcHandler<GetPortfolioDataRequest, GetPortfolioDataResponse>
{
    private readonly ISender _mediator;

    public PortfolioValidationConsumer(ISender mediator)
    {
        _mediator = mediator;
    }

    public async Task<ValidatePortfolioResponse> HandleAsync(
        ValidatePortfolioRequest message,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioQuery(message.PortfolioId), cancellationToken);

        return result.Match(
            _ => new ValidatePortfolioResponse(true, null, null),
            err => new ValidatePortfolioResponse(false, err.Error.Code, err.Error.Description));
    }

    public async Task<GetPortfolioDataResponse> HandleAsync(
        GetPortfolioDataRequest message,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPortfolioQuery(message.PortfolioId), cancellationToken);

        return result.Match(
            success => new GetPortfolioDataResponse(true, null, null, success.CurrentDate),
            err => new GetPortfolioDataResponse(false, err.Error.Code, err.Error.Description));
    }
}