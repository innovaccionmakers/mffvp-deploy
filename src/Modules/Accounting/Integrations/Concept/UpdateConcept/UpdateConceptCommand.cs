using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.Concept.UpdateConcept
{
    [AuditLog]
    public sealed record class UpdateConceptCommand(
        [property: JsonPropertyName("ConceptoId")]
        long ConceptId,

        [property: JsonPropertyName("CuentaDebito")]
        string? DebitAccount,

        [property: JsonPropertyName("CuentaCredito")]
        string? CreditAccount
        ) : ICommand;
}

