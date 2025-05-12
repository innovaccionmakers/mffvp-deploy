using Activations.Domain.Clients;

namespace Activations.Infrastructure.Mocks
{
    internal sealed class InMemoryClientRepository : IClientRepository
    {
        private static readonly List<Client> _clients =
        [
            new Client(IdType: "CC", IdNumber: "8027845", IsActive: true, IsBlocked: false, ProductActivated: true),

            new Client(IdType: "CC", IdNumber: "9000001", IsActive: false, IsBlocked: false, ProductActivated: true),

            new Client(IdType: "TI", IdNumber: "1234567", IsActive: true, IsBlocked: true, ProductActivated: true),

            new Client(IdType: "PP", IdNumber: "A1234567", IsActive: true, IsBlocked: false, ProductActivated: false),
        ];

        public Client? Get(string idType, string idNumber)
            => _clients.Find(c => c.IdType == idType && c.IdNumber == idNumber);
    }
}
