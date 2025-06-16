using Microsoft.EntityFrameworkCore;
using Operations.Domain.TransactionTypes;
using Operations.Infrastructure.Database;
using Common.SharedKernel.Domain;

namespace Operations.Infrastructure.TransactionTypes;

public class TransactionTypeRepository(OperationsDbContext context) : ITransactionTypeRepository
{
    public async Task<IReadOnlyCollection<TransactionType>> GetTransactionTypesByTypeAsync(string type, CancellationToken cancellationToken = default)
    {
        var query = context.ConfigurationParameters
           .Where(cp => cp.Type == type && cp.Status);

        return await query
            .Select(cp => TransactionType.Create(
                cp.ConfigurationParameterId,
                cp.HomologationCode,
                cp.Name,
                cp.Status,
                context.SubtransactionTypes
                    .Where(st => st.Category == cp.Uuid && st.Status == Status.Active)
                    .ToList()
                )).ToListAsync(cancellationToken);
    }
}

