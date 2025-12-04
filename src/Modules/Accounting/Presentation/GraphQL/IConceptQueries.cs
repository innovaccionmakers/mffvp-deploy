using Accounting.Presentation.DTOs;

namespace Accounting.Presentation.GraphQL
{
    public interface IConceptQueries
    {
        Task<IReadOnlyCollection<ConceptDto>> GetConceptsAsync(CancellationToken cancellationToken = default);
    }
}

