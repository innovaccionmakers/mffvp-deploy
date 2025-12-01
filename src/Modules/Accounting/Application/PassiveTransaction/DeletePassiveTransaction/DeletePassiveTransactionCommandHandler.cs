using Accounting.Application.Abstractions.Data;
using Accounting.Application.PassiveTransaction.UpdatePassiveTransaction;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.DeletePassiveTransaction;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.PassiveTransaction.DeletePassiveTransaction
{
    internal class DeletePassiveTransactionCommandHandler(
        IPassiveTransactionRepository passiveTransactionRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdatePassiveTransactionCommandHandler> logger) : ICommandHandler<DeletePassiveTransactionCommand>
    {
        public async Task<Result> Handle(DeletePassiveTransactionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
                var transaction = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(request.PortfolioId, request.TypeOperationsId, cancellationToken);

                if (transaction is null)
                    return Result.Failure(Error.NullValue);

                passiveTransactionRepository.Delete(transaction); 
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al eliminar la transacción pasiva. Error: {Message}", ex.Message); 
                return Result.Failure<GetPassiveTransactionsResponse>(Error.NotFound("0", "No No se puedo eliminar la configuración contable."));
            }
        }
    }
}
