using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountingAccount
{
    public sealed record AccountingAccountResponse(
        [property: JsonPropertyName("Cuenta")]
        string Account,
        [property: JsonPropertyName("Nombre")]
        string Name
        );
}
