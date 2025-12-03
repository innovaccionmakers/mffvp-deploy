using Associate.Domain.ConfigurationParameters;
using Closing.Application.PreClosing.Services.Yield.Dto;
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

internal sealed class GetYieldDetailsByPortfolioIdsAndClosingDateQueryHandler(
    ILogger<GetYieldDetailsByPortfolioIdsAndClosingDateQueryHandler> logger,
    IConfigurationParameterRepository configurationParameterRepository,
    IYieldDetailRepository yieldDetailRepository) : IQueryHandler<GetYieldDetailsByPortfolioIdsAndClosingDateQuery, IReadOnlyCollection<YieldDetailResponse>>
{
    public async Task<Result<IReadOnlyCollection<YieldDetailResponse>>> Handle(GetYieldDetailsByPortfolioIdsAndClosingDateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var conceptJson = await BuildConceptJsonAsync(request.GuidConcept, cancellationToken);
            var yieldDetails = await yieldDetailRepository.GetYieldDetailsByPortfolioIdsAndClosingDateAsync(request.PortfolioIds, request.ClosingDate, request.Source, conceptJson, cancellationToken);

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

    private async Task<string?> BuildConceptJsonAsync(Guid? guidConcept, CancellationToken cancellationToken)
    {
        if (!guidConcept.HasValue)
            return null;

        var adjustmentConceptParam = await configurationParameterRepository.GetByUuidAsync(
            guidConcept.Value,
            cancellationToken);

        if (adjustmentConceptParam?.Metadata == null)
            return null;

        var conceptId = JsonIntegerHelper.ExtractInt32(adjustmentConceptParam.Metadata, "id", defaultValue: 0);
        var conceptName = JsonStringHelper.ExtractString(adjustmentConceptParam.Metadata, "nombre", defaultValue: string.Empty);

        if (conceptId <= 0 || string.IsNullOrWhiteSpace(conceptName))
            return null;

        var conceptDto = new StringEntityDto(conceptId.ToString(), conceptName);
        return JsonSerializer.Serialize(conceptDto);
    }
}

