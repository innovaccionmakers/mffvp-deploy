using Closing.Integrations.PortfolioValuation;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.External.Closing;
using Products.IntegrationEvents.PortfolioValuation;

namespace Products.Infrastructure.External.PortfolioValuations
{
    internal class PortfolioValuationLocator(IRpcClient rpcClient) : IPortfolioValuationLocator
    {
        public async Task<Result<IReadOnlyCollection<PortfolioValuationResponse>>> GetPortfolioValuationAsync(DateTime closingDate, CancellationToken cancellationToken = default)
        {
            var response = await rpcClient.CallAsync<GetPortfolioValuationRequest, GetPortfolioValuationResponse>(new GetPortfolioValuationRequest(closingDate), cancellationToken);

            return response.IsValid
                ? Result.Success<IReadOnlyCollection<PortfolioValuationResponse>>(response.PortfolioValuations
                    .Select(x => new PortfolioValuationResponse(
                        x.PortfolioId,
                        x.ClosingDate,
                        x.Contributions,
                        x.Withdrawals,
                        x.PygBruto,
                        x.Expenses,
                        x.CommissionDay,
                        x.CostDay,
                        x.CreditedYields,
                        x.GrossYieldPerUnit,
                        x.CostPerUnit,
                        x.UnitValue,
                        x.Units,
                        x.AmountPortfolio,
                        x.TrustIds
                    )).ToList())
                : Result.Failure<IReadOnlyCollection<PortfolioValuationResponse>>(Error.Validation(response.Code, response.Message));
        }
    }
}
