using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Closing.Integrations.Closing.RunClosing;

public sealed record CancelClosingCommand(
    [property: JsonPropertyName("IdPortafolio")]
    int PortfolioId,
    [property: JsonPropertyName("FechaCierre")]
    DateTime ClosingDate)
    : ICommand;