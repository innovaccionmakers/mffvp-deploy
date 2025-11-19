using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;
using System.Text.Json.Serialization;

namespace Closing.Integrations.PreClosing.RunSimulation;

[AuditLog]
public sealed record RunSimulationCommand(
    [property: JsonPropertyName("IdPortafolio")]
    int PortfolioId,
    [property: JsonPropertyName("FechaCierre")]
    DateTime ClosingDate,
    [property: JsonPropertyName("EsCierre")]
    bool IsClosing = false
) : ICommand<SimulatedYieldResult>;

public sealed record RunSimulationParameters(
      int PortfolioId,
      DateTime ClosingDate,
      bool IsClosing = false,
      bool IsFirstClosingDay = false,
      bool HasDebitNotes = false  // Indica si el portafolio tiene Rendimientos Extra por Nota de Débito
  );


public sealed record SimulationYieldCalculatorParameters(
    decimal YieldToCredit,
    decimal? PreviousPortfolioValue,
    decimal? PreviousUnitValue,
    decimal? PreviousUnits,
    bool IsFirstClosingDay = false
  );


