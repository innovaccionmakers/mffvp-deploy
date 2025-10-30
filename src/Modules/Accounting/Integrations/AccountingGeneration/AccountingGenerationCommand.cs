using Common.SharedKernel.Application.Messaging;
using MediatR;

namespace Accounting.Integrations.AccountingGeneration;

public sealed record AccountingGenerationCommand(
    string User,
    string ProcessId,
    DateTime StartDate,
    DateTime ProcessDate
    ) : ICommand<Unit>;