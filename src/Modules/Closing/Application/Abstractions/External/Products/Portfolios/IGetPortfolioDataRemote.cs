
using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Products.Portfolios;

public interface IGetPortfolioDataRemote
{
    Task<Result<GetPortfolioClosingDataRemoteResponse>> GetAsync(
      GetPortfolioClosingDataRemoteRequest request,
      CancellationToken cancellationToken);
}

public sealed record GetPortfolioClosingDataRemoteRequest(
    int PortfolioId
);

public sealed record GetPortfolioClosingDataRemoteResponse(
     bool Succeeded,
    int AgileWithdrawalPercentageProtectedBalance,
    string? Code ,
    string? Message
);