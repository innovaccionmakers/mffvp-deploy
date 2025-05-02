using System.Collections.Concurrent;
using Contributions.Application.Abstractions.Lookups;

namespace Contributions.Infrastructure.Mocks
{
    internal sealed class InMemoryLookupService : ILookupService
    {
        private readonly ConcurrentDictionary<(string Table, string Code), bool> _data = new()
        {
            [("IdentificationType", "CC")] = true,
            [("Origin", "A")] = true,
            [("CollectionMethod", "AO")] = true,
            [("PaymentMethod", "TB")] = true,
        };

        private readonly ConcurrentDictionary<string, bool> _originCertification = new()
        {
            ["A"] = true,
        };

        public bool CodeExists(string table, string code)
            => _data.ContainsKey((table, code));

        public bool CodeIsActive(string table, string code)
            => _data.TryGetValue((table, code), out var active) && active;

        public bool OriginRequiresCertification(string originCode)
            => _originCertification.TryGetValue(originCode, out var requires) && requires;
    }
}
