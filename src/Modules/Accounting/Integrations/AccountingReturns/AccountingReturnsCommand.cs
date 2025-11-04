using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountingReturns;

[AuditLog]
public sealed record AccountingReturnsCommand(
    [property: JsonPropertyName("idsPortafolio")]
    IEnumerable<int> PortfolioIds,
    [property: JsonPropertyName("fechaProceso")]
    DateTime ProcessDate
 ) : ICommand<bool>;
