using Associate.Presentation.DTOs;

namespace Associate.Presentation.GraphQL;

public interface IAssociatesExperienceQueries
{
    Task<IReadOnlyCollection<AssociateDto>> GetAllAssociatesAsync(CancellationToken cancellationToken);
    Task<AssociateDto?> GetAssociateByIdAsync(long associateId, CancellationToken cancellationToken = default);
}
