
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Closing.Integrations.Closing.RunClosing;

public sealed record RunClosingCommand(
    [property: JsonPropertyName("IdPortafolio")]
    int PortfolioId,
    [property: JsonPropertyName("FechaCierre")]
    DateTime ClosingDate,
    [property: JsonPropertyName("EsCierre")]
    bool IsClosing = false
) : ICommand<ClosedResult>;