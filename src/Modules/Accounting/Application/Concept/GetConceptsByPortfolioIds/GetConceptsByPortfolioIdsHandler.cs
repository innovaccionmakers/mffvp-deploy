using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
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
            var concept = await conceptsRepository.GetConceptsByPortfolioIdsAsync(query.PortfolioIds, query.Concepts, cancellationToken);

            var response = concept
            .Select(c => new GetConceptsByPortfolioIdsResponse(
                c.PortfolioId,
                c.Name,
                c.DebitAccount,
                c.CreditAccount
                ))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetConceptsByPortfolioIdsResponse>>(response);
        }
    }
}
