using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.YieldDetails;
using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Yields
{
    internal class GetAllAutConceptsQueryHandler(
        ILogger<GetAllFeesQueryHandler> logger,
        IYieldRepository yieldRepository,
        IYieldDetailRepository yieldDetailRepository) : IQueryHandler<GetAllAutConceptsQuery, YieldAutConceptsCompleteResponse>
    {
        public async Task<Result<YieldAutConceptsCompleteResponse>> Handle(GetAllAutConceptsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var yields = await yieldRepository.GetAllAutConceptsByPortfolioIdsAndClosingDateAsync(request.PortfolioIds, request.ClosingDate, cancellationToken);
                if (yields is null || yields.Count == 0)
                {
                    logger.LogWarning("No se encontraron rendimientos para los portafolios y fecha de cierre proporcionadas.");
                    return Result.Failure<YieldAutConceptsCompleteResponse>(new Error("Error", "No se encontraron rendimientos para los portafolios y fecha de cierre proporcionadas.", ErrorType.Validation));
                }

                var yieldDetails = await yieldDetailRepository.GetYieldDetailsAutConceptsAsync(request.PortfolioIds, request.ClosingDate, cancellationToken);
                if (yields is null || yields.Count == 0)
                {
                    logger.LogWarning("No se encontraron detalles rendimientos para los portafolios y fecha de cierre proporcionadas.");
                    return Result.Failure<YieldAutConceptsCompleteResponse>(new Error("Error", "No se encontraron detalles rendimientos para los portafolios y fecha de cierre proporcionadas.", ErrorType.Validation));
                }

                var yieldResponses = yields.Select(y => new YieldAutConceptsResponse(
                    YieldId: y.YieldId,
                    PortfolioId: y.PortfolioId,
                    YieldToCredit: y.YieldToCredit,
                    CreditedYields: y.CreditedYields
                )).ToList();

                var yieldDetailResponse = yieldDetails.Select(yd => new YieldDetailsAutConceptsResponse(
                        yd!.PortfolioId,
                        yd.Income,
                        yd.Expenses
                        )).ToList();

                var completeResponse = new YieldAutConceptsCompleteResponse(
                    yieldResponses,
                    yieldDetailResponse
                );

                return Result.Success<YieldAutConceptsCompleteResponse>(completeResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurrió un error inesperado al obtener los rendimientos.");
                return Result.Failure<YieldAutConceptsCompleteResponse>(new Error("Error", "Ocurrió un error inesperado al obtener los comisiones.", ErrorType.Problem));
            }
        }
    }
}
