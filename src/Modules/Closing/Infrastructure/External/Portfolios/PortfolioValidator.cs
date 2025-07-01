using Closing.Application.Abstractions.External;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.IntegrationEvents.PortfolioValidation;

namespace Closing.Infrastructure.External.Portfolios;

internal sealed class PortfolioValidator(ICapRpcClient rpc) : IPortfolioValidator
{
    public async Task<Result> EnsureExistsAsync(int portfolioId, CancellationToken ct)
    {
        var response = await rpc.CallAsync<
            ValidatePortfolioRequest,
            ValidatePortfolioResponse>(
            nameof(ValidatePortfolioRequest),
            new ValidatePortfolioRequest(portfolioId),
            TimeSpan.FromSeconds(5),
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
            nameof(GetPortfolioDataRequest),
            new GetPortfolioDataRequest(portfolioId),
            TimeSpan.FromSeconds(5),
            ct);

        return response.IsValid
            ? Result.Success(new PortfolioData(portfolioId, response.CurrentDate!.Value))
            : Result.Failure<PortfolioData>(Error.Validation(response.Code, response.Message));
    }
}