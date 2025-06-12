using Common.SharedKernel.Domain;

namespace Customers.Domain.EconomicActivities;

public static class EconomicActivityErrors
{
    public static Error NotFound(string economicactivityId)
    {
        return Error.NotFound(
            "EconomicActivity.NotFound",
            $"The economicactivity with identifier {economicactivityId} was not found"
        );
    }
}