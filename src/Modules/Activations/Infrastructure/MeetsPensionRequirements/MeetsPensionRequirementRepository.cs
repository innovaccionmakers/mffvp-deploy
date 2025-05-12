using Activations.Domain.MeetsPensionRequirements;
using Activations.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Activations.Infrastructure;

internal sealed class MeetsPensionRequirementRepository(ActivationsDbContext context)
    : IMeetsPensionRequirementRepository
{
    public async Task<IReadOnlyCollection<MeetsPensionRequirement>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await context.MeetsPensionRequirements.ToListAsync(cancellationToken);
    }

    public async Task<MeetsPensionRequirement?> GetAsync(int meetspensionrequirementId,
        CancellationToken cancellationToken = default)
    {
        return await context.MeetsPensionRequirements
            .SingleOrDefaultAsync(x => x.MeetsPensionRequirementId == meetspensionrequirementId, cancellationToken);
    }

    public void Insert(MeetsPensionRequirement meetspensionrequirement)
    {
        context.MeetsPensionRequirements.Add(meetspensionrequirement);
    }

    public void Update(MeetsPensionRequirement meetspensionrequirement)
    {
        context.MeetsPensionRequirements.Update(meetspensionrequirement);
    }

    public void Delete(MeetsPensionRequirement meetspensionrequirement)
    {
        context.MeetsPensionRequirements.Remove(meetspensionrequirement);
    }
}