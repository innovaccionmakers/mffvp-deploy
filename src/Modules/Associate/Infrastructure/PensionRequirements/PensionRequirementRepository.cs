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

    public async Task<PensionRequirement?> GetAsync(int ActivateId, CancellationToken cancellationToken = default)
    {
        return await context.PensionRequirements.SingleOrDefaultAsync(x => x.ActivateId == ActivateId);
    }

    public void Insert(PensionRequirement pensionrequirement)
    {
        context.PensionRequirements.Add(pensionrequirement);
    }

    public void Update(PensionRequirement pensionrequirement)
    {
        context.PensionRequirements.Update(pensionrequirement);
    }
}