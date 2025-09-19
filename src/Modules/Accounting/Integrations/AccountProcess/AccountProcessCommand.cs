using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountProcess
{
    [AuditLog]
    public sealed record AccountProcessCommand(
        [property: JsonPropertyName("idsPortafolio")]
        List<int> PortfolioIds,
        [property: JsonPropertyName("fechaProceso")]
        DateTime ProcessDate
        ) : ICommand<string>;
}
