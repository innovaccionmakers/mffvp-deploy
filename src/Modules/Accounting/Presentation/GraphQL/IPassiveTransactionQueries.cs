using Accounting.Presentation.DTOs;
using Common.SharedKernel.Presentation.Results;

namespace Accounting.Presentation.GraphQL
{
    public interface IPassiveTransactionQueries
    {
        Task<GraphqlResult<PassiveTransactionsDto>> GetPassiveTransactionsAsync(
            int PortfolioId,
            long TypeOperationsId,
            CancellationToken cancellationToken = default);
    }
}
