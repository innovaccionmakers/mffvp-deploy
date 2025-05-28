namespace Associate.Domain.PensionRequirements;
public interface IPensionRequirementRepository
{
    Task<IReadOnlyCollection<PensionRequirement>> GetAllAsync(CancellationToken cancellationToken = default);
    void Insert(PensionRequirement pensionrequirement);
    void Update(PensionRequirement pensionrequirement);
}