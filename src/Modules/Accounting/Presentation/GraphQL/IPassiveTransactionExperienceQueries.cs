using Accounting.Presentation.DTOs;
using Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;

namespace Accounting.Presentation.GraphQL
{
    public interface IPassiveTransactionExperienceQueries
    {
        Task<GraphqlResult<PassiveTransactionsDto>> GetPassiveTransactionsAsync(GetPassiveTransactionInput input, CancellationToken cancellationToken = default);
    }
}
