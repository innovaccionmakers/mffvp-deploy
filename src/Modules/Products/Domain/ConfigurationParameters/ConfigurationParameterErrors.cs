using Common.SharedKernel.Core.Primitives;

namespace Products.Domain.ConfigurationParameters;

public static class ConfigurationParameterErrors
{
    public static Error NotFound(long planId)
    {
        return Error.NotFound(
            "ConfigurationParameter.NotFound",
            $"The Configuration Parameter with identifier {planId} was not found"
        );
    }
}