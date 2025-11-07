using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.PassiveTransaction.GetAccountingOperationsPassiveTransaction;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Accounting.Application.PassiveTransaction.GetAccountingOperationsPassiveTransaction
{
    internal class GetAccountingOperationsPassiveTransactionHandler(
    IPassiveTransactionRepository passiveTransactionRepository) : IQueryHandler<GetAccountingOperationsPassiveTransactionQuery, IReadOnlyCollection<GetAccountingOperationsPassiveTransactionResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetAccountingOperationsPassiveTransactionResponse>>> Handle(
            GetAccountingOperationsPassiveTransactionQuery query,
            CancellationToken cancellationToken)
        { 
            var treasury = await passiveTransactionRepository.GetAccountingOperationsAsync(query.PortfolioIds, query.OperationTypeId, cancellationToken);

            var response = treasury
            .Select(t => new GetAccountingOperationsPassiveTransactionResponse(
                t.PortfolioId,
                t.CreditAccount))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetAccountingOperationsPassiveTransactionResponse>>(response);
        }
    }
}
