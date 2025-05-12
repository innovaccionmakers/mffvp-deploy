using Trusts.Domain.Clients;

namespace Trusts.Infrastructure.Mocks;

internal sealed class InMemoryClientRepository : IClientRepository
{
    private static readonly List<Client> _clients =
    [
        new("CC", "8027845", true, false, true),

        new("CC", "9000001", false, false, true),

        new("TI", "1234567", true, true, true),

        new("PP", "A1234567", true, false, false)
    ];

    public Client? Get(string idType, string idNumber)
    {
        return _clients.Find(c => c.IdType == idType && c.IdNumber == idNumber);
    }
}