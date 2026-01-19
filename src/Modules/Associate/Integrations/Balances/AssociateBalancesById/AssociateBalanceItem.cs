using System.Text.Json.Serialization;

namespace Associate.Integrations.Balances.AssociateBalancesById;

public sealed record AssociateBalanceItem(
    [property: JsonPropertyName("IdPortafolio")] string PortfolioId,
    [property: JsonPropertyName("NombrePortafolio")] string PortfolioName,
    [property: JsonPropertyName("IdObjetivo")] int ObjectiveId,
    [property: JsonPropertyName("NombreObjetivo")] string ObjectiveName,
    [property: JsonPropertyName("IdAlternativa")] string AlternativeId,
    [property: JsonPropertyName("NombreAlternativa")] string AlternativeName,
    [property: JsonPropertyName("IdFondo")] string FundId,
    [property: JsonPropertyName("NombreFondo")] string FundName,
    [property: JsonPropertyName("SaldoTotal")] decimal TotalBalance,
    [property: JsonPropertyName("SaldoDisponible")] decimal AvailableAmount,
    [property: JsonPropertyName("SaldoDisponibleRetiroAgil")] decimal AgileWithdrawalAvailable,
    [property: JsonPropertyName("SaldoProtegido")] decimal ProtectedBalance,
    [property: JsonPropertyName("PermiteRetiroAgil")] bool AllowsAgileWithdrawal);
