
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Products.Integrations.Commissions.Queries;
using Products.Integrations.Commissions.Response;
namespace Products.IntegrationEvents.Commission.CommissionsByPortfolio;

public sealed class CommissionsByPortfolioConsumer : IRpcHandler<CommissionsByPortfolioRequest, CommissionsByPortfolioResponse>
{
    private readonly ISender _mediator;
    public CommissionsByPortfolioConsumer(ISender mediator) => _mediator = mediator;

    public async Task<CommissionsByPortfolioResponse> HandleAsync(
    CommissionsByPortfolioRequest request,
    CancellationToken cancellationToken)
    {
        var commissionsResult = await _mediator.Send(
       new GetCommissionsByPortfolioIdQuery(request.PortfolioId),
       cancellationToken);

        return commissionsResult.Match(
  val => new CommissionsByPortfolioResponse(
      true,
      null,
      null,
      val),
  error => new CommissionsByPortfolioResponse(
      false,
      error.Code,
      error.Description,
      Array.Empty<GetCommissionsByPortfolioIdResponse>())
);




    }
}
