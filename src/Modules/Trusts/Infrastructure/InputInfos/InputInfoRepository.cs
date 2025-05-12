using Microsoft.EntityFrameworkCore;
using Trusts.Domain.InputInfos;
using Trusts.Infrastructure.Database;

namespace Trusts.Infrastructure;

internal sealed class InputInfoRepository(TrustsDbContext context) : IInputInfoRepository
{
    public async Task<IReadOnlyCollection<InputInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.InputInfos.ToListAsync(cancellationToken);
    }

    public async Task<InputInfo?> GetAsync(Guid inputinfoId, CancellationToken cancellationToken = default)
    {
        return await context.InputInfos
            .SingleOrDefaultAsync(x => x.InputInfoId == inputinfoId, cancellationToken);
    }

    public void Insert(InputInfo inputinfo)
    {
        context.InputInfos.Add(inputinfo);
    }

    public void Update(InputInfo inputinfo)
    {
        context.InputInfos.Update(inputinfo);
    }

    public void Delete(InputInfo inputinfo)
    {
        context.InputInfos.Remove(inputinfo);
    }
}