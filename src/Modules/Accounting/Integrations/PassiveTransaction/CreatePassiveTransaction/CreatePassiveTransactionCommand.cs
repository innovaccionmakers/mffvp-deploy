using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.PassiveTransaction.CreatePassiveTransaction
{
    [AuditLog]
    public sealed record class CreatePassiveTransactionCommand(
        [property: JsonPropertyName("PortafolioId")]
        int PortfolioId,

        [property: JsonPropertyName("TipoOperacionesId")]
        long TypeOperationsId,

        [property: JsonPropertyName("CuentaDebito")]
        string? DebitAccount,

        [property: JsonPropertyName("CuentaCredito")]
        string? CreditAccount,

        [property: JsonPropertyName("CuentaContraCredito")]
        string? ContraCreditAccount,

        [property: JsonPropertyName("CuentaContraDebito")]
        string? ContraDebitAccount
        ) : ICommand;
}
