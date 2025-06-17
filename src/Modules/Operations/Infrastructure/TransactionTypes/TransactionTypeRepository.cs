using Microsoft.EntityFrameworkCore;
using Operations.Domain.TransactionTypes;
using Operations.Infrastructure.Database;
using Common.SharedKernel.Domain;

namespace Operations.Infrastructure.TransactionTypes;

public class TransactionTypeRepository(OperationsDbContext context) : ITransactionTypeRepository
{
    const string ConfigurationParameterType = "TipoTransaccion";

    public async Task<IReadOnlyCollection<TransactionType>> GetTransactionTypesAsync(CancellationToken cancellationToken = default)
    {
        var query = context.ConfigurationParameters
           .Where(cp => cp.Type == ConfigurationParameterType && cp.Status);

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

