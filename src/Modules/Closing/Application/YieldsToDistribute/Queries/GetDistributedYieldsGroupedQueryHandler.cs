using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.YieldsToDistribute;
using Closing.Integrations.YieldsToDistribute;
using Closing.Integrations.YieldsToDistribute.Queries;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Closing.Application.YieldsToDistribute.Queries;

internal sealed class GetDistributedYieldsGroupedQueryHandler(
    ILogger<GetDistributedYieldsGroupedQueryHandler> logger,
    IConfigurationParameterRepository configurationParameterRepository,
    IYieldToDistributeRepository yieldToDistributeRepository) : IQueryHandler<GetDistributedYieldsGroupedQuery, IReadOnlyCollection<DistributedYieldGroupResponse>>
{
    public async Task<Result<IReadOnlyCollection<DistributedYieldGroupResponse>>> Handle(GetDistributedYieldsGroupedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var adjustmentConceptParam = await configurationParameterRepository.GetByUuidAsync(
                ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote,
                cancellationToken);

            string? conceptJson = null;
            if (adjustmentConceptParam?.Metadata != null)
            {
                var conceptId = JsonIntegerHelper.ExtractInt32(adjustmentConceptParam.Metadata, "id", defaultValue: 0);
                var conceptName = JsonStringHelper.ExtractString(adjustmentConceptParam.Metadata, "nombre", defaultValue: string.Empty);

                if (conceptId > 0 && !string.IsNullOrWhiteSpace(conceptName))
                {
                    var conceptDto = new StringEntityDto(conceptId.ToString(), conceptName);
                    conceptJson = JsonSerializer.Serialize(conceptDto);
                }
            }

            var distributedYields = await yieldToDistributeRepository
                .GetDistributedYieldsByConceptAsync(request.PortfolioIds, request.ClosingDate, conceptJson, cancellationToken);

            if (distributedYields is null || distributedYields.Count == 0)
            {
                logger.LogWarning("No se encontraron rendimientos distribuidos para los portafolios, fecha de cierre y concepto proporcionados.");
                return Result.Failure<IReadOnlyCollection<DistributedYieldGroupResponse>>(new Error("Error", "No se encontraron rendimientos distribuidos para los portafolios, fecha de cierre y concepto proporcionados.", ErrorType.Validation));
            }

            return distributedYields
                .GroupBy(x => new { x.ClosingDate, x.PortfolioId, ConceptJson = JsonDocumentHelper.NormalizeJson(x.Concept) })
                .Select(dy => new DistributedYieldGroupResponse(
                    ClosinDate: dy.Key.ClosingDate,
                    PortofolioId: dy.Key.PortfolioId,
                    Concept: JsonDocument.Parse(dy.Key.ConceptJson),
                    TotalYieldAmount: dy.Sum(y => y.YieldAmount)
                )).ToList();

        } catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error inesperado al obtener los rendimientos distribuidos agrupados.");
            return Result.Failure<IReadOnlyCollection<DistributedYieldGroupResponse>>(new Error("Error", "Ocurrió un error inesperado al obtener los rendimientos distribuidos agrupados.", ErrorType.Problem));
        }
    }
}
