using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.PreClosing.RunSimulation;
public sealed record RunSimulationCommand(
    int PortfolioId,
    DateTime ClosingDate,
    bool IsClosing = false
) : ICommand<SimulatedYieldResult>;

public sealed record RunSimulationParameters(
      int PortfolioId,
      DateTime ClosingDate,
      bool IsClosing = false,
      bool IsFirstClosingDay = false
  );


public sealed record SimulationYieldCalculatorParameters(
    decimal YieldToCredit,
    decimal? PreviousPortfolioValue,
    decimal? PreviousUnitValue,
    decimal? PreviousUnits,
    bool IsFirstClosingDay = false
  );


