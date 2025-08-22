using Common.SharedKernel.Core.Primitives;

namespace Trusts.Domain.ConfigurationParameters;

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