using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountingOperations
{
    [AuditLog]
    public sealed record AccountingOperationsCommand(
        [property: JsonPropertyName("idsPortafolio")]
        IEnumerable<int> PortfolioIds,
        [property: JsonPropertyName("fechaProceso")]
        DateTime ProcessDate
        ) : ICommand<bool>;
}
