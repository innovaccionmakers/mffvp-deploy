using Closing.Integrations.PortfolioValuations.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.PortfolioValuation;

public sealed class GetPortfolioValuationInfoConsumer(ISender sender)
    : IRpcHandler<GetPortfolioValuationInfoRequest, GetPortfolioValuationInfoResponse>
{
    public async Task<GetPortfolioValuationInfoResponse> HandleAsync(
        GetPortfolioValuationInfoRequest request,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetPortfolioValuationInfoQuery(request.PortfolioId, request.ClosingDate), ct);

        if (!result.IsSuccess)
        {
            return new GetPortfolioValuationInfoResponse(false, result.Error.Code, result.Error.Description, null);
        }

        var valuation = result.Value;

        var valuationInfo = new PortfolioValuationInfoDto(
            valuation.PortfolioId,
            valuation.ClosingDate,
            valuation.UnitValue);

        return new GetPortfolioValuationInfoResponse(true, null, null, valuationInfo);
    }
}
