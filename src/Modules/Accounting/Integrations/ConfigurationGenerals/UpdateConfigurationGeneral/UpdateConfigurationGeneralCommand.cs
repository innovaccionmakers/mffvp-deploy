using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConfigurationGenerals.UpdateConfigurationGeneral
{
    [AuditLog]
    public sealed record class UpdateConfigurationGeneralCommand(
            int PortfolioId,
            string AccountingCode,
            string CostCenter
        ) : ICommand;
}
