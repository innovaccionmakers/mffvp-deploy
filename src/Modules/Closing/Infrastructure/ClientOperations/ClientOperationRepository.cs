using Microsoft.EntityFrameworkCore;
using Closing.Domain.ClientOperations;
using Closing.Infrastructure.Database;

namespace Closing.Infrastructure.ClientOperations;

internal sealed class ClientOperationRepository(ClosingDbContext context) : IClientOperationRepository
{
    public void Insert(ClientOperation clientOperation)
    {
        context.ClientOperations.Add(clientOperation);
    }
} 