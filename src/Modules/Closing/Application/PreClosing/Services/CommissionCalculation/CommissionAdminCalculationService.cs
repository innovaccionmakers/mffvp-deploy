
using Closing.Domain.Constants;
using Closing.Domain.PortfolioValuations;
using Common.SharedKernel.Domain;

namespace Closing.Application.PreClosing.Services.CommissionCalculation
{
    public class CommissionAdminCalculationService: ICommissionAdminCalculationService
    {
        private readonly IPortfolioValuationRepository _portfolioValuationRepository;
        public CommissionAdminCalculationService(IPortfolioValuationRepository portfolioValuationRepository)
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
        /// la fecha anterior al día de ejecución de la simulación.
        /// </remarks>
        /// <returns></returns>
        public async Task<Result<decimal>> CalculateAsync(int portfolioId, DateTime closingDate, decimal commissionPercentage, CancellationToken ct)
        {
            var portfolioValuationPreviousDate = await _portfolioValuationRepository.GetValuationAsync(portfolioId, closingDate.AddDays(-1), ct);
            if (portfolioValuationPreviousDate == null)
            {
                return Result.Failure<decimal>(new Error("000",$"No se encontró la valoración del portafolio para el ID {portfolioId} en la fecha {closingDate.AddDays(-1)}.", ErrorType.Failure));
            }
            var dailyPercentage = ((commissionPercentage / 100) / CommissionRateBase.Days365);
            var portfolioAmountPreviousDate = portfolioValuationPreviousDate.Amount;
            var commission = portfolioAmountPreviousDate * dailyPercentage;
            return commission;
        }
    }
}
