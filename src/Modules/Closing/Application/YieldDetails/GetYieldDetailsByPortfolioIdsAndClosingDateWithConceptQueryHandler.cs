using Closing.Application.Helpers;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.YieldDetails;
using Closing.Integrations.YieldDetails;
using Closing.Integrations.YieldDetails.Queries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.YieldDetails;

internal sealed class GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandler(
    ILogger<GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQueryHandler> logger,
    IConfigurationParameterRepository configurationParameterRepository,
    IYieldDetailRepository yieldDetailRepository) : IQueryHandler<GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQuery, IReadOnlyCollection<YieldDetailResponse>>
{
    public async Task<Result<IReadOnlyCollection<YieldDetailResponse>>> Handle(GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var conceptJsons = await ConceptJsonHelper.BuildConceptJsonsAsync(configurationParameterRepository, request.GuidConcepts, cancellationToken);

            var yieldDetails = await yieldDetailRepository.GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(
                request.PortfolioIds,
                request.ClosingDate,
                request.Source,
                conceptJsons,
                cancellationToken);

            if (yieldDetails is null || yieldDetails.Count == 0)
            {
                logger.LogWarning("No se encontraron detalles de rendimiento para los portafolios y fecha de cierre proporcionadas.");
                return Result.Failure<IReadOnlyCollection<YieldDetailResponse>>(new Error("Error", "No se encontraron detalles de rendimiento para los portafolios y fecha de cierre proporcionadas.", ErrorType.Validation));
            }

            var yieldDetailResponses = yieldDetails
                .GroupBy(yd => new { yd.PortfolioId, yd.ClosingDate })
                .Select(g => new YieldDetailResponse(
                    YieldDetailId: 0,
                    PortfolioId: g.Key.PortfolioId,
                    Income: g.Sum(yd => yd.Income),
                    Expenses: g.Sum(yd => yd.Expenses),
                    Commissions: g.Sum(yd => yd.Commissions),
                    ClosingDate: g.Key.ClosingDate,
                    ProcessDate: g.Max(yd => yd.ProcessDate),
                    IsClosed: true
                ))
                .ToList();

            return Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetailResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener detalles de rendimiento por portafolios y fecha de cierre.");
            return Result.Failure<IReadOnlyCollection<YieldDetailResponse>>(new Error("Error", $"Error al obtener detalles de rendimiento: {ex.Message}", ErrorType.Failure));
        }
    }
}

