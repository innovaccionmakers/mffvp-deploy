using Common.SharedKernel.Application.Attributes;
using Common.SharedKernel.Application.Messaging;

namespace Accounting.Integrations.ConfigurationGenerals.GetConfigurationGenerals
{
    [AuditLog]
    public sealed record class GetConfigurationGeneralsQuery() : IQuery<IReadOnlyCollection<GetConfigurationGeneralsResponse>>;
}
