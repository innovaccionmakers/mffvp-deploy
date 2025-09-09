
namespace Trusts.Domain.Trusts.TrustYield;
public sealed record TrustYieldUpdateDiagnostics(
    bool MatchesExpectedBalance,
    bool MatchesCapitalPlusYield,
    decimal NewTotal,
    decimal ExpectedCapitalPlusYield);
