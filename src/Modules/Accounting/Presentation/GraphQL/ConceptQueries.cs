using Accounting.Integrations.Concept.GetConcepts;
using Accounting.Presentation.DTOs;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class ConceptQueries(
        ISender mediator) : IConceptQueries
    {
        public async Task<IReadOnlyCollection<ConceptDto>> GetConceptsAsync(CancellationToken cancellationToken = default)
        {
            var result = await mediator.Send(new GetConceptsQuery(), cancellationToken);

            if (!result.IsSuccess || result.Value == null)
                return [];

            var concepts = result.Value;

            return concepts.Select(x => new ConceptDto(
                x.ConceptId,
                x.PortfolioId,
                x.Name,
                x.DebitAccount,
                x.CreditAccount
            )).ToList();
        }
    }
}

