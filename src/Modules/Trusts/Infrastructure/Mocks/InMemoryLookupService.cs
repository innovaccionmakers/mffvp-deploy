using System.Collections.Concurrent;
using Trusts.Application.Abstractions.Lookups;

namespace Trusts.Infrastructure.Mocks;

internal sealed class InMemoryLookupService : ILookupService
{
    private readonly ConcurrentDictionary<(string Table, string Code), bool> _data = new()
    {
        [("IdentificationType", "CC")] = true,
        [("IdentificationType", "TI")] = true,
        [("IdentificationType", "PP")] = false,

        [("Origin", "A")] = true,
        [("Origin", "B")] = false,

        [("CollectionMethod", "AO")] = true,
        [("CollectionMethod", "BO")] = false,

        [("PaymentMethod", "TB")] = true,
        [("PaymentMethod", "CB")] = false
    };

    private readonly ConcurrentDictionary<string, bool> _originCertification = new()
    {
        ["A"] = true,
        ["B"] = false
    };

    public bool CodeExists(string table, string code)
    {
        return _data.ContainsKey((table, code));
    }

    public bool CodeIsActive(string table, string code)
    {
        return _data.TryGetValue((table, code), out var active) && active;
    }

    public bool OriginRequiresCertification(string originCode)
    {
        return _originCertification.TryGetValue(originCode, out var requires) && requires;
    }
}