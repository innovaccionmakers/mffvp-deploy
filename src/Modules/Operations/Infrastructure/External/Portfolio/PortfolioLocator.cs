using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.Application.Abstractions.External;

using Products.IntegrationEvents.Portfolio;

namespace Operations.Infrastructure.External.Portfolio;

internal sealed class PortfolioLocator(IRpcClient rpc) : IPortfolioLocator
{
    public async Task<Result<(long PortfolioId, string Name, DateTime CurrentDate)>> FindByPortfolioIdAsync(int PortfolioId, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetPortfolioByIdRequest,
            GetPortfolioByIdResponse>(
            new GetPortfolioByIdRequest(PortfolioId),
            ct);

        return rc.Succeeded
            ? Result.Success((rc.Portfolio!.PortfolioId, rc.Portfolio.Name, rc.Portfolio.CurrentDate))
            : Result.Failure<(long, string, DateTime)>(Error.Validation(rc.Code, rc.Message));
    }
}
