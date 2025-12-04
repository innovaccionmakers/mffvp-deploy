using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.YieldDetails;
using Closing.Integrations.YieldDetails;
using Closing.Integrations.YieldDetails.Queries;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json;

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
            var conceptJsons = await BuildConceptJsonsAsync(request.GuidConcepts, cancellationToken);

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

    private async Task<IEnumerable<string>> BuildConceptJsonsAsync(IEnumerable<Guid> guidConcepts, CancellationToken cancellationToken)
    {
        if (guidConcepts == null || !guidConcepts.Any())
            return Enumerable.Empty<string>();

        var conceptParams = await configurationParameterRepository.GetReadOnlyByUuidsAsync(guidConcepts, cancellationToken);

        var conceptJsons = new List<string>();

        foreach (var conceptParam in conceptParams.Values)
        {
            if (conceptParam?.Metadata == null)
                continue;

            var conceptId = JsonIntegerHelper.ExtractInt32(conceptParam.Metadata, "id", defaultValue: 0);
            var conceptName = JsonStringHelper.ExtractString(conceptParam.Metadata, "nombre", defaultValue: string.Empty);

            if (conceptId <= 0 || string.IsNullOrWhiteSpace(conceptName))
                continue;

            var conceptDto = new StringEntityDto(conceptId.ToString(), conceptName);
            conceptJsons.Add(JsonSerializer.Serialize(conceptDto));
        }

        return conceptJsons;
    }
}

