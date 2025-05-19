using Common.SharedKernel.Domain;

namespace Associate.Domain.ConfigurationParameters;
public static class ConfigurationParameterErrors
{
    public static Error NotFound(int configurationparameterId) =>
        Error.NotFound(
            "ConfigurationParameter.NotFound",
            $"The configurationparameter with identifier {configurationparameterId} was not found"
        );
}