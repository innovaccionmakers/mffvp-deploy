using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Products.Domain.Portfolios;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.Queries;

namespace Products.Application.Portfolios.QueriesHandlers;

internal sealed class AreAllPortfoliosClosedQueryHandler(
    IPortfolioRepository portfolioRepository,
    ILogger<AreAllPortfoliosClosedQueryHandler> logger)
    : IQueryHandler<AreAllPortfoliosClosedQuery, bool>
{
    public async Task<Result<bool>> Handle(
        AreAllPortfoliosClosedQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var areAllClosed = await portfolioRepository.AreAllPortfoliosClosedAsync(
                request.PortfolioIds,
                request.Date,
                cancellationToken);

            return Result.Success(areAllClosed);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error inesperado al verificar el estado de cierre de los portafolios");
            return Result.Failure<bool>(
                Error.Problem("Portfolio", "Ocurrió un error inesperado al verificar el estado de cierre de los portafolios"));
        }
    }
}

