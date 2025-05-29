using Microsoft.EntityFrameworkCore;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.Database;

namespace Operations.Infrastructure.ClientOperations;

internal sealed class ClientOperationRepository(OperationsDbContext context) : IClientOperationRepository
{
    public async Task<IReadOnlyCollection<ClientOperation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations.ToListAsync(cancellationToken);
    }

    public async Task<ClientOperation?> GetAsync(long clientoperationId, CancellationToken cancellationToken = default)
    {
        return await context.ClientOperations
            .SingleOrDefaultAsync(x => x.ClientOperationId == clientoperationId, cancellationToken);
    }

    public void Insert(ClientOperation clientoperation)
    {
        context.ClientOperations.Add(clientoperation);
    }

    public void Update(ClientOperation clientoperation)
    {
        context.ClientOperations.Update(clientoperation);
    }

    public void Delete(ClientOperation clientoperation)
    {
        context.ClientOperations.Remove(clientoperation);
    }
    
    public async Task<bool> ExistsContributionAsync(
        int affiliateId, int objectiveId, int portfolioId, CancellationToken ct)
    {
        return await context.ClientOperations
            .Where(co => co.AffiliateId == affiliateId &&
                         co.ObjectiveId  == objectiveId  &&
                         co.PortfolioId  == portfolioId)
            .Join(context.ConfigurationParameters,
                co => co.SubtransactionTypeId,
                st => st.ConfigurationParameterId,
                (co, st) => new { co, st })
            .Join(context.ConfigurationParameters,
                x => x.st.ParentId,
                parent => parent.ConfigurationParameterId,
                (x, parent) => parent.Name)
            .AnyAsync(name => name == "Aporte", ct);
    }

}