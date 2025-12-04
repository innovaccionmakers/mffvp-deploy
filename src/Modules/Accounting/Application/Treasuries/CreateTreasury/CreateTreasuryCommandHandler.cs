using Accounting.Application.Abstractions.Data;
using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.CreateTreasury;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Treasuries.CreateTreasury
{
    internal class CreateTreasuryCommandHandler(
        ITreasuryRepository treasuryRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateTreasuryCommandHandler> logger) : ICommandHandler<CreateTreasuryCommand>
    {
        public async Task<Result> Handle(CreateTreasuryCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var treasury = Domain.Treasuries.Treasury.Create(
                request.PortfolioId,
                request.BankAccount,
                request.DebitAccount,
                request.CreditAccount
                );

                treasuryRepository.Insert(treasury.Value);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al crear la tesoreria. Error: {Message}", ex.Message);
                return Result.Failure(Error.NotFound("0", "Error al crear la tesoreria"));
            }
        }
    }
}
