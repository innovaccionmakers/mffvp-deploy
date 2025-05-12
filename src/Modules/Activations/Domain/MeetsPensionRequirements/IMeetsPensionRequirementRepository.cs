namespace Activations.Domain.MeetsPensionRequirements;

public interface IMeetsPensionRequirementRepository
{
    Task<IReadOnlyCollection<MeetsPensionRequirement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<MeetsPensionRequirement?> GetAsync(int id, CancellationToken cancellationToken = default);
    void Insert(MeetsPensionRequirement meetspensionrequirement);
    void Update(MeetsPensionRequirement meetspensionrequirement);
    void Delete(MeetsPensionRequirement meetspensionrequirement);
}