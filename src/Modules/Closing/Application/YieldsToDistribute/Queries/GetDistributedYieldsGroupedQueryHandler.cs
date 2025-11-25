using Closing.Domain.YieldsToDistribute;
using Closing.Integrations.YieldsToDistribute;
using Closing.Integrations.YieldsToDistribute.Queries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Products.Domain.Portfolios;

namespace Closing.Application.YieldsToDistribute.Queries;

internal sealed class GetDistributedYieldsGroupedQueryHandler(
    ILogger<GetDistributedYieldsGroupedQueryHandler> logger,
    IYieldToDistributeRepository yieldToDistributeRepository) : IQueryHandler<GetDistributedYieldsGroupedQuery, IReadOnlyCollection<DistributedYieldGroupResponse>>
{
    public async Task<Result<IReadOnlyCollection<DistributedYieldGroupResponse>>> Handle(GetDistributedYieldsGroupedQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var distributedYields = await yieldToDistributeRepository
                .GetDistributedYieldsByConceptAsync(request.PortfolioIds, request.ClosingDate, request.Concept, cancellationToken);
            if (distributedYields is null || distributedYields.Count == 0)
            {
                logger.LogWarning("No se encontraron rendimientos distribuidos para los portafolios, fecha de cierre y concepto proporcionados.");
                return Result.Failure<IReadOnlyCollection<DistributedYieldGroupResponse>>(new Error("Error", "No se encontraron rendimientos distribuidos para los portafolios, fecha de cierre y concepto proporcionados.", ErrorType.Validation));
            }

            return distributedYields
                .GroupBy(x => new { x.ClosingDate, x.PortfolioId, x.Concept })
                .Select(dy => new DistributedYieldGroupResponse(
                    ClosinDate: dy.Key.ClosingDate,
                    PortofolioId: dy.Key.PortfolioId,
                    Concept: dy.Key.Concept,
                    TotalYieldAmount: dy.Sum(y => y.YieldAmount)
                )).ToList();

        } catch (Exception ex)
        {
            logger.LogError(ex, "Ocurrió un error inesperado al obtener los rendimientos distribuidos agrupados.");
            return Result.Failure<IReadOnlyCollection<DistributedYieldGroupResponse>>(new Error("Error", "Ocurrió un error inesperado al obtener los rendimientos distribuidos agrupados.", ErrorType.Problem));
        }
    }
}
