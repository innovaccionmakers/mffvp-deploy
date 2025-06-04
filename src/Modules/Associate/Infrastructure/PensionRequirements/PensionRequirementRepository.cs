using Microsoft.EntityFrameworkCore;
using Associate.Domain.PensionRequirements;
using Associate.Infrastructure.Database;

namespace Associate.Infrastructure;

internal sealed class PensionRequirementRepository(AssociateDbContext context) : IPensionRequirementRepository
{
    public async Task<IReadOnlyCollection<PensionRequirement>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.PensionRequirements.ToListAsync(cancellationToken);
    }

    public async Task<PensionRequirement?> GetAsync(int PensionRequirementId, CancellationToken cancellationToken = default)
    {
        return await context.PensionRequirements.SingleOrDefaultAsync(x => x.PensionRequirementId == PensionRequirementId);
    }

    public void Insert(PensionRequirement pensionrequirement)
    {
        context.PensionRequirements.Add(pensionrequirement);
    }

    public void Update(PensionRequirement pensionrequirement)
    {
        context.PensionRequirements.Update(pensionrequirement);
    }

    public async Task<int> DeactivateExistingRequirementsAsync(int activateId, CancellationToken cancellationToken)
    {
        return await context.PensionRequirements
            .Where(r => r.ActivateId == activateId && r.Status == true)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.Status, false), cancellationToken);
    }
}