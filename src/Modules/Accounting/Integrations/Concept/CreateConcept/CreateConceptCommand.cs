using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.Concept.CreateConcept
{
    public sealed record class CreateConceptCommand(
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

