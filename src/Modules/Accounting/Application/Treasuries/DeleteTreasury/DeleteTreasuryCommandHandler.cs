using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.DeleteTreasury;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Treasuries.DeleteTreasury
{
    internal class DeleteTreasuryCommandHandler(
        ITreasuryRepository treasuryRepository,
        IPortfolioLocator portfolioLocator,
        IUnitOfWork unitOfWork,
        ILogger<DeleteTreasuryCommandHandler> logger) : ICommandHandler<DeleteTreasuryCommand>
    {
        public async Task<Result> Handle(DeleteTreasuryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var portfolioResult = await portfolioLocator.GetPortfolioInformationAsync(request.PortfolioId, cancellationToken);

                if (portfolioResult.IsFailure)
                {
                    logger.LogError("No se pudo obtener la información del portafolio {PortfolioId}: {Error}", request.PortfolioId, portfolioResult.Error);
                    return Result.Failure(Error.NotFound(portfolioResult.Error.Code, portfolioResult.Error.Description));
                }

                await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
                var treasury = await treasuryRepository.GetTreasuryAsync(request.PortfolioId, request.BankAccount, cancellationToken);

                if (treasury is null)
                    return Result.Failure(Error.NotFound("0", "No hay registros de tesorería para eliminar."));

                treasuryRepository.Delete(treasury);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al eliminar la tesorería. Error: {Message}", ex.Message);
                return Result.Failure(Error.NotFound("0", "No se pudo eliminar la tesorería."));
            }
        }
    }
}
