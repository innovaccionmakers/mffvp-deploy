using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Accounting.Presentation.DTOs;
using Common.SharedKernel.Presentation.Results;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class PassiveTransactionQueries(
        ISender mediator) : IPassiveTransactionQueries
    {
        public async Task<GraphqlResult<PassiveTransactionsDto>> GetPassiveTransactionsAsync(int PortfolioId, long TypeOperationsId, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<PassiveTransactionsDto>();
            try
            {
                var response = await mediator.Send(new GetPassiveTransactionsQuery(PortfolioId, TypeOperationsId), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                    throw new InvalidOperationException("Failed to retrieve transaction passive.");

                var getPassive = response.Value;

                var passiveTransactions = new PassiveTransactionsDto(
                    getPassive.PassiveTransactionId,
                    getPassive.DebitAccount,
                    getPassive.CreditAccount,
                    getPassive.ContraCreditAccount,
                    getPassive.ContraDebitAccount
                );

                result.Data = passiveTransactions;

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
