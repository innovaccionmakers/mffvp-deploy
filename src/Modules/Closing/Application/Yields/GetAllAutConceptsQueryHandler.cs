using Closing.Domain.Yields;
using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Yields
{
    internal class GetAllAutConceptsQueryHandler(
        ILogger<GetAllFeesQueryHandler> logger,
        IYieldRepository yieldRepository) : IQueryHandler<GetAllAutConceptsQuery, IReadOnlyCollection<YieldAutConceptsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<YieldAutConceptsResponse>>> Handle(GetAllAutConceptsQuery request, CancellationToken cancellationToken) {

            try
            {
                var yields = await yieldRepository.GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(request.PortfolioIds, request.ClosingDate, cancellationToken);

                if (yields is null || yields.Count == 0)
                {
                    logger.LogWarning("No se encontraron rendimientos para los portafolios y fecha de cierre proporcionadas.");
                    return Result.Failure<IReadOnlyCollection<YieldAutConceptsResponse>>(new Error("Error", "No se encontraron rendimientos para los portafolios y fecha de cierre proporcionadas.", ErrorType.Validation));
                }

                var yieldResponses = yields.Select(y => new YieldAutConceptsResponse(
                    YieldId: y.YieldId,
                    PortfolioId: y.PortfolioId,
                    YieldToCredit: y.YieldToCredit,
                    CreditedYields: y.CreditedYields
                )).ToList();

                return Result.Success<IReadOnlyCollection<YieldAutConceptsResponse>>(yieldResponses);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurrió un error inesperado al obtener los rendimientos.");
                return Result.Failure<IReadOnlyCollection<YieldAutConceptsResponse>>(new Error("Error", "Ocurrió un error inesperado al obtener los comisiones.", ErrorType.Problem));
            }
        }
    }
}
