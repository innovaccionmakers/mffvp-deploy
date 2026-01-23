using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransaction;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.PassiveTransaction.GetPassiveTransaction
{
    internal class GetPassiveTransactionQueryHandler(
        IPassiveTransactionRepository passiveTransactionRepository,
        ILogger<GetPassiveTransactionQueryHandler> logger) : IQueryHandler<GetPassiveTransactionQuery, GetPassiveTransactionResponse>
    {
        public async Task<Result<GetPassiveTransactionResponse>> Handle(GetPassiveTransactionQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var passiveTransactions = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(request.PortfolioId, request.TypeOperationsId, cancellationToken);

                if (passiveTransactions is null)
                    return Result.Failure<GetPassiveTransactionResponse>(Error.NotFound("0", "No se encontró registro de la configuración contable."));

                var response = new GetPassiveTransactionResponse(
                        passiveTransactions.PassiveTransactionId,
                        passiveTransactions.DebitAccount,
                        passiveTransactions.CreditAccount,
                        passiveTransactions.ContraCreditAccount,
                        passiveTransactions.ContraDebitAccount
                        );

                return Result.Success(response);

            }
            catch (Exception ex)
            {
                logger.LogError("Error al obtener las transacciones pasivas. Error: {Message}", ex.Message);
                return Result.Failure<GetPassiveTransactionResponse>(Error.NotFound("0", "No hay configuración contable."));
            }
        }
    }
}
