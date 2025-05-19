using Common.SharedKernel.Domain;

namespace Associate.Domain.ConfigurationParameters;
public sealed class ConfigurationParameterCreatedDomainEvent(int configurationparameterId) : DomainEvent
{
    public int ConfigurationParameterId { get; } = configurationparameterId;
}