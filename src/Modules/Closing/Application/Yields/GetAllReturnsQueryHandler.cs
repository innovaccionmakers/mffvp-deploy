using Closing.Domain.Yields;
using Closing.Integrations.Yields;
using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Yields;

internal class GetAllReturnsQueryHandler(
    ILogger<GetAllReturnsQueryHandler> logger,
    IYieldRepository yieldRepository) : IQueryHandler<GetAllReturnsQuery, IReadOnlyCollection<YieldResponse>>
{
    public async Task<Result<IReadOnlyCollection<YieldResponse>>> Handle(GetAllReturnsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var yields = await yieldRepository.GetYieldsByPortfolioIdsAndClosingDateAsync(request.PortfolioIds, request.ClosingDate, cancellationToken);

            if (yields is null || yields.Count == 0)
            {
                logger.LogWarning("No se encontraron rendimientos para los portafolios y fecha de cierre proporcionadas.");
                return Result.Failure<IReadOnlyCollection<YieldResponse>>(new Error("Error", "No se encontraron rendimientos para los portafolios y fecha de cierre proporcionadas.", ErrorType.Validation));
            }

            var yieldResponses = yields.Select(y => new YieldResponse(
                YieldId: y.YieldId,
                PortfolioId: y.PortfolioId,
                Income: y.Income,
                Expenses: y.Expenses,
                Commissions: y.Commissions,
                Costs: y.Costs,
                YieldToCredit: y.YieldToCredit,
                CreditedYields: y.CreditedYields,
                ClosingDate: y.ClosingDate,
                ProcessDate: y.ProcessDate,
                IsClosed: y.IsClosed
            )).ToList();

            return Result.Success<IReadOnlyCollection<YieldResponse>>(yieldResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error inesperado al obtener los comisiones.");
            return Result.Failure<IReadOnlyCollection<YieldResponse>>(new Error("Error", "Ocurrió un error inesperado al obtener los comisiones.", ErrorType.Problem));
        }
    }
}
