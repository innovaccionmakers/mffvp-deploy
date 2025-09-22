using Accounting.Domain.AccountingAssistants;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingAssistants.Commands;

public sealed record AddAccountingEntitiesCommand(
    IEnumerable<AccountingAssistant> AccountingAssistants
) : ICommand<bool>;
