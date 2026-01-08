using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.CreateTreasury;
using Accounting.Integrations.Treasury.GetTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Treasuries.CreateTreasury
{
    internal class CreateTreasuryCommandHandler(
        ITreasuryRepository treasuryRepository,
        IPortfolioLocator portfolioLocator,
        IUnitOfWork unitOfWork,
        ILogger<CreateTreasuryCommandHandler> logger) : ICommandHandler<CreateTreasuryCommand>
    {
        public async Task<Result> Handle(CreateTreasuryCommand request, CancellationToken cancellationToken)
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

                if (treasury != null)
                    return Result.Failure<GetTreasuryResponse>(Error.NotFound("0", "Ya existe un registro de tesorería con esta cuenta bancaria."));

                var result = Domain.Treasuries.Treasury.Create(
                request.PortfolioId,
                request.BankAccount,
                request.DebitAccount,
                request.CreditAccount
                );

                treasuryRepository.Insert(result.Value);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al crear la tesoreria. Error: {Message}", ex.Message);
                return Result.Failure(Error.NotFound("0", "Error al crear la tesorería."));
            }
        }
    }
}
