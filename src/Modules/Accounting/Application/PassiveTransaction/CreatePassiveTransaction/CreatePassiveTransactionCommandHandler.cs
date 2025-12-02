using Accounting.Application.Abstractions.Data;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.CreatePassiveTransaction;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.PassiveTransaction.CreatePassiveTransaction
{
    internal class CreatePassiveTransactionCommandHandler(
        IPassiveTransactionRepository passiveTransactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreatePassiveTransactionCommandHandler> logger) : ICommandHandler<CreatePassiveTransactionCommand>
    {
        public async Task<Result> Handle(CreatePassiveTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = Domain.PassiveTransactions.PassiveTransaction.Create(
                    request.PortfolioId,
                    request.TypeOperationsId,
                    request.DebitAccount,
                    request.CreditAccount,
                    request.ContraCreditAccount,
                    request.ContraDebitAccount
                    );

                passiveTransactionRepository.Insert(result.Value);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al crear la transacción pasiva: Error: {Message}", ex.Message);
                return Result.Failure<GetPassiveTransactionsResponse>(Error.NotFound("0", "No se puedo crear la configuración contable."));
            }
        }
    }
}
