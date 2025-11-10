using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountingFees;

public sealed record AccountingFeesCommand(
    [property: JsonPropertyName("idsPortafolio")]
    IEnumerable<int> PortfolioIds,
    [property: JsonPropertyName("fechaProceso")]
    DateTime ProcessDate
) : ICommand<bool>;
