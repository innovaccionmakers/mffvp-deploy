
using Common.SharedKernel.Application.Messaging;

namespace Closing.Integrations.Closing.RunClosing;

public sealed record ConfirmClosingCommand(int PortfolioId, DateTime ClosingDate)
    : ICommand<ClosedResult>;