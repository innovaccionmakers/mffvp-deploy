using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.PassiveTransaction.GetPassiveTransactions
{
    internal class GetPassiveTransactionsQueryHandler(
        IPassiveTransactionRepository passiveTransactionRepository,
        ILogger<GetPassiveTransactionsQueryHandler> logger) : IQueryHandler<GetPassiveTransactionsQuery, IReadOnlyCollection<GetPassiveTransactionsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetPassiveTransactionsResponse>>> Handle(GetPassiveTransactionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var passiveTransactions = await passiveTransactionRepository.GetPassiveTransactionsAsync(cancellationToken);

                if (passiveTransactions is null)
                    return Result.Failure<IReadOnlyCollection<GetPassiveTransactionsResponse>>(Error.NotFound("0", "No se encontró registro de la configuración contable."));

                var response = passiveTransactions.Select(pt => new GetPassiveTransactionsResponse(
                        pt.PassiveTransactionId,
                        pt.DebitAccount,
                        pt.CreditAccount,
                        pt.ContraCreditAccount,
                        pt.ContraDebitAccount
                        )).ToList();

                return Result.Success<IReadOnlyCollection<GetPassiveTransactionsResponse>>(response);

            }
            catch (Exception ex)
            {
                logger.LogError("Error al obtener las transacciones pasivas. Error: {Message}", ex.Message);
                return Result.Failure< IReadOnlyCollection<GetPassiveTransactionsResponse>>(Error.NotFound("0", "No hay configuración contable."));
            }
        }
    }
}
