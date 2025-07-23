using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Closing.Integrations.Closing.AbortClosing;

public sealed record AbortClosingCommand(
    [property: JsonPropertyName("IdPortafolio")] int PortfolioId,
    [property: JsonPropertyName("FechaCierre")] DateTime ClosingDate
) : ICommand<bool>;