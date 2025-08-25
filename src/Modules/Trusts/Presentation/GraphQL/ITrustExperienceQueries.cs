namespace Trusts.Presentation.GraphQL;

public interface ITrustExperienceQueries
{
    Task<int> GetParticipantAsync(IEnumerable<long> trustIds, CancellationToken cancellationToken = default);
}
