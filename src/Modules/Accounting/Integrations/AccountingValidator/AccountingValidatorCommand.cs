using Common.SharedKernel.Application.Messaging;
using MediatR;

namespace Accounting.Integrations.AccountingValidator;

public sealed record AccountingValidatorCommand(
    string User,
    string ProcessType,
    bool IsSuccess,
    string? ErrorMessage,
    string ProcessId,
    DateTime StartDate,
    DateTime ProcessDate,
    IEnumerable<int> PortfolioIds
) : ICommand<Unit>;
