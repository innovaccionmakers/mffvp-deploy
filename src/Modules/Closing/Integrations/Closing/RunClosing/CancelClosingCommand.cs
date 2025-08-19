using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Closing.Integrations.Closing.RunClosing;

[AuditLog]
public sealed record CancelClosingCommand(
    [property: JsonPropertyName("IdPortafolio")]
    int PortfolioId,
    [property: JsonPropertyName("FechaCierre")]
    DateTime ClosingDate)
    : ICommand;