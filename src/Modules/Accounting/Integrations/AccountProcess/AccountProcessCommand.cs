using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountProcess
{
    [AuditLog]
    public sealed record AccountProcessCommand(
        [property: JsonPropertyName("idsPortafolio")]
        IEnumerable<int> PortfolioIds,
        [property: JsonPropertyName("fechaProceso")]
        DateOnly ProcessDate
        ) : ICommand<string>;
}
