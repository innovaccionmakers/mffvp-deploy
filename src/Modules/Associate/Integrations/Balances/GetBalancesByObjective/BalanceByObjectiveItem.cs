using System.Text.Json.Serialization;

namespace Associate.Integrations.Balances.GetBalancesByObjective;

public sealed record BalanceByObjectiveItem(
    [property: JsonPropertyName("entidad")] string Entity,
    [property: JsonPropertyName("codigoFondo")] string FundCode,
    [property: JsonPropertyName("codigoParticipacion")] string ParticipationCode,
    [property: JsonPropertyName("nombreFondo")] string FundName,
    [property: JsonPropertyName("nombreParticipacion")] string ParticipationName,
    [property: JsonPropertyName("numeroProducto")] int ProductNumber,
    [property: JsonPropertyName("numeroDocumentoTitularAfiliado")] string AffiliateDocument,
    [property: JsonPropertyName("nombreTitularFondo")] string HolderName,
    [property: JsonPropertyName("saldoDisponible")] decimal AvailableBalance,
    [property: JsonPropertyName("saldoTotal")] decimal TotalBalance,
    [property: JsonPropertyName("fechaVencimiento")] string ExpirationDate,
    [property: JsonPropertyName("bloqueo")] string Blocked,
    [property: JsonPropertyName("permiteCredito")] string AllowsCredit,
    [property: JsonPropertyName("permiteDebito")] string AllowsDebit,
    [property: JsonPropertyName("permiteConsulta")] string AllowsInquiry,
    [property: JsonPropertyName("permiteNomina")] string AllowsPayroll,
    [property: JsonPropertyName("permiteProveedores")] string AllowsSuppliers,
    [property: JsonPropertyName("tieneCotitulares")] string HasCoOwners,
    [property: JsonPropertyName("canalAperturaFondo")] string FundOpeningChannel
);
