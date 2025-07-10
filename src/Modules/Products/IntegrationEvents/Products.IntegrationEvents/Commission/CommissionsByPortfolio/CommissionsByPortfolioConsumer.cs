
using Common.SharedKernel.Application.Messaging;
using DotNetCore.CAP;
using MediatR;
using Products.IntegrationEvents.Commission.CommissionsByPortfolio;
using Products.Integrations.Commissions.Queries;
using Products.Integrations.Commissions.Response;
namespace Products.IntegrationEvents.Commission.GetCommissionsByPortfolio
{
    public sealed class CommissionsByPortfolioConsumer : ICapSubscribe
    {
        private readonly ISender _mediator;
        public CommissionsByPortfolioConsumer(ISender mediator) => _mediator = mediator;

        [CapSubscribe(nameof(CommissionsByPortfolioRequest))]
        public async Task<CommissionsByPortfolioResponse> HandleAsync(
        CommissionsByPortfolioRequest request,
        [FromCap] CapHeader header,
        CancellationToken cancellationToken)
        {
            var correlationId = header[CapRpcClient.Headers.CorrelationId];
            header.AddResponseHeader(CapRpcClient.Headers.CorrelationId, correlationId);

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
}
