using Closing.Domain.YieldDetails;
using Closing.Integrations.YieldDetails;
using Closing.Integrations.YieldDetails.Queries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.YieldDetails;

internal sealed class GetYieldDetailsByPortfolioIdsAndClosingDateQueryHandler(
    ILogger<GetYieldDetailsByPortfolioIdsAndClosingDateQueryHandler> logger,
    IYieldDetailRepository yieldDetailRepository) : IQueryHandler<GetYieldDetailsByPortfolioIdsAndClosingDateQuery, IReadOnlyCollection<YieldDetailResponse>>
{
    public async Task<Result<IReadOnlyCollection<YieldDetailResponse>>> Handle(GetYieldDetailsByPortfolioIdsAndClosingDateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var yieldDetails = await yieldDetailRepository.GetYieldDetailsByPortfolioIdsAndClosingDateAsync(request.PortfolioIds, request.ClosingDate, cancellationToken);

            if (yieldDetails is null || yieldDetails.Count == 0)
            {
                logger.LogWarning("No se encontraron detalles de rendimiento para los portafolios y fecha de cierre proporcionadas.");
                return Result.Failure<IReadOnlyCollection<YieldDetailResponse>>(new Error("Error", "No se encontraron detalles de rendimiento para los portafolios y fecha de cierre proporcionadas.", ErrorType.Validation));
            }

            var yieldDetailResponses = yieldDetails.Select(yd => new YieldDetailResponse(
                YieldDetailId: yd.YieldDetailId,
                PortfolioId: yd.PortfolioId,
                Income: yd.Income,
                Expenses: yd.Expenses,
                Commissions: yd.Commissions,
                ClosingDate: yd.ClosingDate,
                ProcessDate: yd.ProcessDate,
                IsClosed: yd.IsClosed
            )).ToList();

            return Result.Success<IReadOnlyCollection<YieldDetailResponse>>(yieldDetailResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener detalles de rendimiento por portafolios y fecha de cierre.");
            return Result.Failure<IReadOnlyCollection<YieldDetailResponse>>(new Error("Error", $"Error al obtener detalles de rendimiento: {ex.Message}", ErrorType.Failure));
        }
    }
}

