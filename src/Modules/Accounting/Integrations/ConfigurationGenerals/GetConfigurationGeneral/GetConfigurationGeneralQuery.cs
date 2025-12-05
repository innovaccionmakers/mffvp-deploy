using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConfigurationGenerals.GetConfigurationGeneral
{
    [AuditLog]
    public sealed record class GetConfigurationGeneralQuery(int PortfolioId) : IQuery<GetConfigurationGeneralResponse>;
}
