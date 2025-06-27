using Associate.Presentation.DTOs;

namespace Associate.Presentation.GraphQL;

public interface IAssociatesExperienceQueries
{
    Task<IReadOnlyCollection<AssociateDto>> GetAllAssociatesAsync(CancellationToken cancellationToken);
}
