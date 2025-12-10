using Closing.Application.Helpers;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Domain.YieldsToDistribute;
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
        IYieldDetailRepository yieldDetailRepository,
        IYieldToDistributeRepository yieldToDistributeRepository,
        IConfigurationParameterRepository configurationParameterRepository) : IQueryHandler<GetAllAutConceptsQuery, YieldAutConceptsCompleteResponse>
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

                var guidConcepts = new[]
                {
                    ConfigurationParameterUuids.Closing.YieldAdjustmentIncome,
                    ConfigurationParameterUuids.Closing.YieldAdjustmentExpense
                };
                var conceptJsons = await ConceptJsonHelper.BuildConceptJsonsAsync(configurationParameterRepository, guidConcepts, cancellationToken);

                var yieldDetails = await yieldDetailRepository.GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptsAsync(
                    request.PortfolioIds,
                    request.ClosingDate,
                    YieldsSources.AutomaticConcept,
                    conceptJsons,
                    cancellationToken);
                if (yieldDetails is null || yieldDetails.Count == 0)
                {
                    logger.LogWarning("No se encontraron detalles rendimientos para los portafolios y fecha de cierre proporcionadas.");
                    return Result.Failure<YieldAutConceptsCompleteResponse>(new Error("Error", "No se encontraron detalles rendimientos para los portafolios y fecha de cierre proporcionadas.", ErrorType.Validation));
                }
                
                var creditNoteConceptJson = await ConceptJsonHelper.BuildConceptJsonsAsync(
                    configurationParameterRepository,
                    new[] { ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote },
                    cancellationToken);

                
                var distributedYields = await yieldToDistributeRepository.GetDistributedYieldsByConceptAsync(
                    request.PortfolioIds,
                    request.ClosingDate,
                    creditNoteConceptJson.FirstOrDefault(),
                    cancellationToken);


                var yieldToDistributeByPortfolio = distributedYields
                    .GroupBy(yd => yd.PortfolioId)
                    .ToDictionary(g => g.Key, g => g.Sum(yd => yd.YieldAmount));

                var yieldResponses = yields.Select(y => new YieldAutConceptsResponse(
                    YieldId: y.YieldId,
                    PortfolioId: y.PortfolioId,
                    YieldToCredit: y.YieldToCredit,
                    CreditedYields: y.CreditedYields,
                    YieldToDistributedValue: yieldToDistributeByPortfolio.TryGetValue(y.PortfolioId, out var distributedValue)
                        ? distributedValue
                        : 0m
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

                return Result.Success(completeResponse);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ocurrió un error inesperado al obtener los rendimientos.");
                return Result.Failure<YieldAutConceptsCompleteResponse>(new Error("Error", "Ocurrió un error inesperado al obtener los comisiones.", ErrorType.Problem));
            }
        }
    }
}
