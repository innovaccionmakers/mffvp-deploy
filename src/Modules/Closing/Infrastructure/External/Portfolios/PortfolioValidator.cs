using Closing.Application.Abstractions.External;

using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

using Products.IntegrationEvents.PortfolioValidation;

namespace Closing.Infrastructure.External.Portfolios;

internal sealed class PortfolioValidator(IRpcClient rpc) : IPortfolioValidator
{
    public async Task<Result> EnsureExistsAsync(int portfolioId, CancellationToken ct)
    {
        var response = await rpc.CallAsync<
            ValidatePortfolioRequest,
            ValidatePortfolioResponse>(
            new ValidatePortfolioRequest(portfolioId),
            ct);

        return response.IsValid
            ? Result.Success()
            : Result.Failure(Error.Validation(response.Code, response.Message));
    }

    public async Task<Result<PortfolioData>> GetPortfolioDataAsync(int portfolioId, CancellationToken ct)
    {
        var response = await rpc.CallAsync<
            GetPortfolioDataRequest,
            GetPortfolioDataResponse>(
            new GetPortfolioDataRequest(portfolioId),
            ct);

        return response.IsValid
            ? Result.Success(new PortfolioData(portfolioId, response.CurrentDate!.Value))
            : Result.Failure<PortfolioData>(Error.Validation(response.Code, response.Message));
    }
}