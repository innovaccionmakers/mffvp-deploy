using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountingConcepts
{
    [AuditLog]
    public sealed record AccountingConceptsCommand(
        [property: JsonPropertyName("idsPortafolio")]
        IEnumerable<int> PortfolioIds,
        [property: JsonPropertyName("fechaProceso")]
        DateTime ProcessDate
        ) : ICommand<bool>;
}
