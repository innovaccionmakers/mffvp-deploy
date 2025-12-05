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
        public async Task<GraphqlResult<PassiveTransactionsDto>> GetPassiveTransactionsAsync(GetPassiveTransactionInput input, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<PassiveTransactionsDto>();
            try
            {
                var response = await mediator.Send(new GetPassiveTransactionsQuery(input.PortfolioId, input.TypeOperationsId), cancellationToken);

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

                result.Data = passiveTransactions;

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
