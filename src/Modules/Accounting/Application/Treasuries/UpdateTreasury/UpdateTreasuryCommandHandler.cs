using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.UpdateTreasury;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Treasuries.UpdateTreasury
{
    internal class UpdateTreasuryCommandHandler(
        ITreasuryRepository treasuryRepository,
        IPortfolioLocator portfolioLocator,
        IUnitOfWork unitOfWork,
        ILogger<UpdateTreasuryCommandHandler> logger) : ICommandHandler<UpdateTreasuryCommand>
    {
        public async Task<Result> Handle(UpdateTreasuryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(request.PortfolioId, cancellationToken);

                if (portfolioResult.IsFailure)
                {
                    logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}", request.PortfolioId, portfolioResult.Error);
                    return Result.Failure(Error.NotFound(portfolioResult.Error.Code, portfolioResult.Error.Description));
                }
                var treasury = await treasuryRepository.GetTreasuryAsync(request.PortfolioId, request.BankAccount, cancellationToken);

                if (treasury is null)
                    return Result.Failure(Error.NotFound("0", "No hay registros de tesorería para actualizar."));

                treasury.UpdateDetails(
                    request.DebitAccount,
                    request.CreditAccount
                    );

                treasuryRepository.Update(treasury);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al actualizar la tesorería. Error: {Message}", ex.Message);
                return Result.Failure(Error.NotFound("0", "No se pudo actualizar la tesorería"));
            }
        }
    }
}
