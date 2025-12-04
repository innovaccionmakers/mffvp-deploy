using Accounting.Application.Abstractions.Data;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Accounting.Integrations.PassiveTransaction.UpdatePassiveTransaction;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.PassiveTransaction.UpdatePassiveTransaction
{
    internal class UpdatePassiveTransactionCommandHandler(
        IPassiveTransactionRepository passiveTransactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePassiveTransactionCommandHandler> logger) : ICommandHandler<UpdatePassiveTransactionCommand>
    {
        public async Task<Result> Handle(UpdatePassiveTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var transaction = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(request.PortfolioId, request.TypeOperationsId, cancellationToken);

                if (transaction is null)
                    return Result.Failure(Error.NotFound("0", "No hay configuración contable para actualizar."));

                transaction.UpdateDetails(
                    request.PortfolioId,
                    request.TypeOperationsId,
                    request.DebitAccount,
                    request.CreditAccount,
                    request.ContraCreditAccount,
                    request.ContraDebitAccount);

                passiveTransactionRepository.Update(transaction);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al actualizar la transacción pasiva. Error: {Message}", ex.Message);
                return Result.Failure<GetPassiveTransactionsResponse>(Error.NotFound("0", "No se puedo actualizar la configuración contable."));
            }
        }
    }
}
