using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConfigurationGenerals.DeleteConfigurationGeneral
{
    [AuditLog]
    public sealed record class DeleteConfigurationGeneralCommand(
            int PortfolioId
        ) : ICommand;
}
