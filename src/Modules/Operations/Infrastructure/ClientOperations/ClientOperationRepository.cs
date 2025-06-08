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
        int affiliateId,
        int objectiveId,
        int portfolioId,
        CancellationToken ct)
    {
        const string contributionLabel = "Aporte";

        return await context.ClientOperations
            .Where(op =>
                op.AffiliateId == affiliateId &&
                op.ObjectiveId  == objectiveId &&
                op.PortfolioId  == portfolioId)
            
            .Join(context.SubtransactionTypes,
                op  => op.SubtransactionTypeId,
                st  => st.SubtransactionTypeId,
                (op, st) => st)
            
            .Join(context.ConfigurationParameters,
                st  => st.Category,
                cp  => cp.Uuid,
                (st, cp) => cp.Name)
            
            .AnyAsync(name => name == contributionLabel, ct);
    }
}