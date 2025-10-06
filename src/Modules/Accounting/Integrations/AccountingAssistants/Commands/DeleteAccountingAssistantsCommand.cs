using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.AccountingAssistants.Commands
{
    public sealed record class DeleteAccountingAssistantsCommand() : ICommand<bool>;
}
