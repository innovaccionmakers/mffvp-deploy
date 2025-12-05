
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;

namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class GetPortfolioInfoForClosingQueryHandler(IPortfolioRepository portfolioRepository) : IQueryHandler<GetPortfolioInfoForClosingQuery, PortfolioInfoForClosingResponse>
{
    public async Task<Result<PortfolioInfoForClosingResponse>> Handle(GetPortfolioInfoForClosingQuery request, CancellationToken cancellationToken)
    {
        if (request is null)
        {
            return Result.Failure<PortfolioInfoForClosingResponse>(
                Error.Validation(
                    "GetPortfolioInfoForClosing.NullRequest",
                    "La petición no puede ser null."));
        }

        var agileWithdrawalPercentageProtectedBalance =
            await portfolioRepository.GetAgileWithdrawalPercentageProtectedBalanceAsync(
                request.PortfolioId,
                cancellationToken);

        var response = new PortfolioInfoForClosingResponse(
            request.PortfolioId,
            agileWithdrawalPercentageProtectedBalance);

        return Result.Success(response);
    }

}
