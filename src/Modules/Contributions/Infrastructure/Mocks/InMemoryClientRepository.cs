using Contributions.Domain.Clients;

namespace Contributions.Infrastructure.Mocks
{
    internal sealed class InMemoryClientRepository : IClientRepository
    {
        private static readonly List<Client> _clients =
        [
            new Client(IdType: "CC", IdNumber: "8027845", IsActive: true, IsBlocked: false, ProductActivated: true),
            ];
        public Client? Get(string idType, string idNumber)
            => _clients.Find(c => c.IdType == idType && c.IdNumber == idNumber);
    }
}
