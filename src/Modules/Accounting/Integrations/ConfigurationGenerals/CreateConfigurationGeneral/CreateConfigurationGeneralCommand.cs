using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConfigurationGenerals.CreateConfigurationGeneral
{
    [AuditLog]
    public sealed record class CreateConfigurationGeneralCommand(
            int PortfolioId,
            string AccountingCode,
            string CostCenter
        ) : ICommand;
}
