using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.PassiveTransaction.GetPassiveTransactions
{
    internal class GetPassiveTransactionsQueryHandler(
        IPassiveTransactionRepository passiveTransactionRepository,
        ILogger<GetPassiveTransactionsQueryHandler> logger) : IQueryHandler<GetPassiveTransactionsQuery, GetPassiveTransactionsResponse>
    {
        public async Task<Result<GetPassiveTransactionsResponse>> Handle(GetPassiveTransactionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var passiveTransactions = await passiveTransactionRepository.GetByPortfolioIdAndOperationTypeAsync(request.PortfolioId, request.TypeOperationsId, cancellationToken);

                var response = new GetPassiveTransactionsResponse(
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
                throw;
            }
        }
    }
}
