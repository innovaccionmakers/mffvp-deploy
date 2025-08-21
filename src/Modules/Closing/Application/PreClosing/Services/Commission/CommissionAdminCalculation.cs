using Closing.Application.PreClosing.Services.Commission.Constants;
using Closing.Application.PreClosing.Services.Commission.Interfaces;
using Closing.Domain.PortfolioValuations;

using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.Commission
{
    public class CommissionAdminCalculation: ICommissionAdminCalculation
    {
        private readonly IPortfolioValuationRepository _portfolioValuationRepository;
        public CommissionAdminCalculation(IPortfolioValuationRepository portfolioValuationRepository)
        {
            _portfolioValuationRepository = portfolioValuationRepository;
        }
        /// <summary>
        /// Calcula la comisión administrativa para un portafolio en una fecha de cierre específica.
        /// </summary>
        /// <param name="portfolioId"></param>
        /// <param name="closingDate"></param>
        /// <param name="commissionPercentage"></param>
        /// <param name="ct"></param>
        /// <remarks>
        /// Para el cálculo de la comisión de administración, se toma el valor del fondo del día anterior, 
        /// que existe en la tabla valoración_portafolio para el portafolio y 
        /// fecha anterior al día de ejecución de la simulación.
        /// </remarks>
        /// <returns></returns>
        public async Task<Result<decimal>> CalculateAsync(
            int portfolioId,
            DateTime closingDate,
            decimal commissionPercentage,
            CancellationToken ct)
        {
            var previousDate = closingDate.AddDays(-1);

            var valuationResult = await _portfolioValuationRepository
                .GetReadOnlyByPortfolioAndDateAsync(portfolioId, previousDate, ct);

            if (valuationResult is null)
            {
                return Result.Failure<decimal>(new Error(
                    code: "000",
                    description: $"No se encontró la valoración del portafolio para el ID {portfolioId} en la fecha {previousDate:yyyy-MM-dd}.",
                    ErrorType.Failure));
            }

            var dailyRate = CalculateDailyCommissionRate(commissionPercentage);
            var commissionAmount = valuationResult.Amount * dailyRate;

            return Result.Success(commissionAmount);
        }

        private static decimal CalculateDailyCommissionRate(decimal annualPercentage)
        {
            const int DaysInYear = CommissionRateBase.Days365; // 365 días base
            return (annualPercentage / 100) / DaysInYear;
        }

    }
}
