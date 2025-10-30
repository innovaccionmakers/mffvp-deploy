using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AutomaticConcepts
{
    [AuditLog]
    public sealed record class AutomaticConceptsCommand(
        [property: JsonPropertyName("idsPortafolio")]
        IEnumerable<int> PortfolioIds,
        [property: JsonPropertyName("fechaProceso")]
        DateTime ProcessDate
        ) : ICommand<bool>;
}
