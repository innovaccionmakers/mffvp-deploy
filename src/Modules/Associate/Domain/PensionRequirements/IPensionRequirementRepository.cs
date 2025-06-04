namespace Associate.Domain.PensionRequirements;

public interface IPensionRequirementRepository
{
    Task<IReadOnlyCollection<PensionRequirement>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PensionRequirement?> GetAsync(int ActivateId, CancellationToken cancellationToken = default);
    void Insert(PensionRequirement pensionrequirement);
    void Update(PensionRequirement pensionrequirement);
    Task<int> DeactivateExistingRequirementsAsync(int activateId, CancellationToken cancellationToken);
}