
using Closing.Application.Abstractions.External;
using Common.SharedKernel.Domain;
using System.Numerics;

namespace Closing.Application.PreClosing.Services.Validator
{
    public class SimulationValidationServices(
    IPortfolioValidator portfolioValidator
)
    {
        /*** Validaciones de Portafolio ***/
        public async Task<Result> ValidatePortfolioAsync(int portfolioId, CancellationToken cancellationToken)
        {
      
            if (portfolioId <= 0)
                return Result.Failure<bool>(Error.Validation("PortfolioId", "El identificador del portafolio debe ser mayor a cero."));
            if (portfolioId > int.MaxValue)
                return Result.Failure<bool>(Error.Validation("PortfolioId", "El identificador del portafolio debe ser menor al valor máximo para un entero: " + int.MaxValue.ToString() + "." ));
            //validar que la fecha de cierre sea la fecha actual + 1  -- sino, presentar error 
            //if (portfolioId <= 0)
            //    return Result.Failure(Error.Validation("PortfolioId", "El identificador del portafolio debe ser mayor a cero."));
            //var validationResult = await portfolioValidator
            //    .EnsureExistsAsync(portfolioId, cancellationToken);
            //if (!validationResult.IsSuccess)
            //    return Result.Failure(validationResult.Error!);
            return Result.Success();
        }

        public async Task<Result> ValidateClosingDateAsync(int portfolioId, DateTime closingDate )
        {

            if (closingDate <= DateTime.MinValue || closingDate >= DateTime.MaxValue)
                return Result.Failure<bool>(Error.Validation("ClosingDate", "La fecha de cierre debe ser una fecha válida."));
            return Result.Success();

        }
    }
}
