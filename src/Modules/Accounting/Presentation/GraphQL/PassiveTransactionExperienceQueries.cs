using Accounting.Integrations.PassiveTransaction.GetPassiveTransaction;
using Accounting.Integrations.PassiveTransaction.GetPassiveTransactions;
using Accounting.Presentation.DTOs;
using Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class PassiveTransactionExperienceQueries(
        ISender mediator) : IPassiveTransactionExperienceQueries
    {
        public async Task<GraphqlResult<IReadOnlyCollection<PassiveTransactionsDto>>> GetPassiveTransactionsAsync(GetPassiveTransactionInput input, CancellationToken cancellationToken = default)
        {
            if (input.PortfolioId != null && input.TypeOperationId != null)
                return await GetPassiveTransaction(input.PortfolioId ?? default, input.TypeOperationId ?? default, cancellationToken);

            return await GetPassiveTransactions(cancellationToken);
        }

        private async Task<GraphqlResult<IReadOnlyCollection<PassiveTransactionsDto>>> GetPassiveTransaction(int portfolioId, long typeOperationId, CancellationToken cancellationToken)
        {
            var result = new GraphqlResult<IReadOnlyCollection<PassiveTransactionsDto>>();
            try
            {
                var listPassiveTransactions = new List<PassiveTransactionsDto>();
                var response = await mediator.Send(new GetPassiveTransactionQuery(portfolioId, typeOperationId), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                {
                    result.AddError(response.Error);
                    return result;
                }

                var getPassive = response.Value;

                var passiveTransactions = new PassiveTransactionsDto(
                    getPassive.PassiveTransactionId,
                    getPassive.DebitAccount,
                    getPassive.CreditAccount,
                    getPassive.ContraCreditAccount,
                    getPassive.ContraDebitAccount
                );

                listPassiveTransactions.Add(passiveTransactions);
                result.Data = listPassiveTransactions;

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        private async Task<GraphqlResult<IReadOnlyCollection<PassiveTransactionsDto>>> GetPassiveTransactions(CancellationToken cancellationToken)
        {
            var result = new GraphqlResult<IReadOnlyCollection<PassiveTransactionsDto>>();
            try
            {
                var response = await mediator.Send(new GetPassiveTransactionsQuery(), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                {
                    result.AddError(response.Error);
                    return result;
                }

                var listPassiveTransactions = response.Value.Select(pt => new PassiveTransactionsDto(
                    pt.PassiveTransactionId,
                    pt.DebitAccount,
                    pt.CreditAccount,
                    pt.ContraCreditAccount,
                    pt.ContraDebitAccount
                )).ToList();

                result.Data = listPassiveTransactions;

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }

        }
    }
}
