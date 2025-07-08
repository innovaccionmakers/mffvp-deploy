using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.PreClosingSimulation.RunSimulation;
    public sealed record RunSimulationCommand(
        int PortfolioId,
        DateTime ClosingDate,
        bool IsClosing = false
    ) : ICommand<bool>;
