using Common.SharedKernel.Domain;

namespace Customers.Domain.EconomicActivities;
public static class EconomicActivityErrors
{
    public static Error NotFound(int economicactivityId) =>
        Error.NotFound(
            "EconomicActivity.NotFound",
            $"The economicactivity with identifier {economicactivityId} was not found"
        );
}