using Common.SharedKernel.Domain;

namespace Products.Domain.ConfigurationParameters;

public static class ConfigurationParameterErrors
{
    public static Error NotFound(long planId) =>
        Error.NotFound(
            "ConfigurationParameter.NotFound",
            $"The Configuration Parameter with identifier {planId} was not found"
        );
}