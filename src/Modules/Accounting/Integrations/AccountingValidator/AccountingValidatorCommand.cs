using Common.SharedKernel.Application.Messaging;
using MediatR;

namespace Accounting.Integrations.AccountingValidator;

public sealed record AccountingValidatorCommand(
    string ProcessType,
    bool IsSuccess,
    string? ErrorMessage,
    Guid ProcessId,
    DateTime ProcessDate,
    IEnumerable<int> PortfolioIds
) : ICommand<Unit>;
