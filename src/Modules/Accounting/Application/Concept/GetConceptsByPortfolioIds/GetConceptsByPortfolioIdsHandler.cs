using Accounting.Domain.Concepts;
using Accounting.Integrations.Treasuries.GetConceptsByPortfolioIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Accounting.Application.Treasuries.GetConceptsByPortfolioIds
{
    internal class GetConceptsByPortfolioIdsHandler(
    IConceptsRepository conceptsRepository) : IQueryHandler<GetConceptsByPortfolioIdsQuery, IReadOnlyCollection<GetConceptsByPortfolioIdsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetConceptsByPortfolioIdsResponse>>> Handle(
            GetConceptsByPortfolioIdsQuery query,
            CancellationToken cancellationToken)
        { 
            var concept = await conceptsRepository.GetConceptsByPortfolioIdsAsync(query.PortfolioIds, cancellationToken);

            var response = concept
            .Select(c => new GetConceptsByPortfolioIdsResponse(
                c.PortfolioId,
                c.ContraCreditAccount,
                c.ContraDebitAccount,
                c.CreditAccount,
                c.DebitAccount
                ))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetConceptsByPortfolioIdsResponse>>(response);
        }
    }
}
