using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Operations.Application.Abstractions.External;

using Products.IntegrationEvents.Portfolio;

namespace Operations.Infrastructure.External.Portfolio;

internal sealed class PortfolioLocator(IRpcClient rpc) : IPortfolioLocator
{
    public async Task<Result<(long PortfolioId, string Name, DateTime CurrentDate)>> FindByHomologatedCodeAsync(string homologatedCodePortfolio, CancellationToken ct)
    {
        var rc = await rpc.CallAsync<
            GetPortfolioByHomologatedCodeRequest,
            GetPortfolioByHomologatedCodeResponse>(
            new GetPortfolioByHomologatedCodeRequest(homologatedCodePortfolio),
            ct);

        return rc.Succeeded
            ? Result.Success((rc.Portfolio!.PortfolioId, rc.Portfolio.Name, rc.Portfolio.CurrentDate))
            : Result.Failure<(long, string, DateTime)>(Error.Validation(rc.Code, rc.Message));
    }

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

    public async Task<Result<string>> GetHomologateCodeByObjetiveIdAsync(int objetiveId, CancellationToken ct)
    {        
        var rc = await rpc.CallAsync<
            GetHomologateCodeByObjetiveIdRequest,
            GetHomologateCodeByObjetiveIdResponse>(
            new GetHomologateCodeByObjetiveIdRequest(objetiveId),
            ct);
        return rc.Succeeded
            ? Result.Success<string>(rc.HomologatedCode)
            : Result.Failure<string>(Error.Validation(rc.Code, rc.Message));
    }
}
