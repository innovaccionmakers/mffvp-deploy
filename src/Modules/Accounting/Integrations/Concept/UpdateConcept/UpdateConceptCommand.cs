using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.Concept.UpdateConcept
{
    public sealed record class UpdateConceptCommand(
        [property: JsonPropertyName("PortafolioId")]
        int PortfolioId,

        [property: JsonPropertyName("Nombre")]
        string Name,

        [property: JsonPropertyName("CuentaDebito")]
        string? DebitAccount,

        [property: JsonPropertyName("CuentaCredito")]
        string? CreditAccount
        ) : ICommand;
}

