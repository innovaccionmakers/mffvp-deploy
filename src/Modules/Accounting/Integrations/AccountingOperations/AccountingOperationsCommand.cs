using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Accounting.Integrations.AccountingOperations
{
    public sealed record AccountingOperationsCommand(
        [property: JsonPropertyName("idsPortafolio")]
        IEnumerable<int> PortfolioIds,
        [property: JsonPropertyName("fechaProceso")]
        DateTime ProcessDate
        ) : ICommand<bool>;
}
