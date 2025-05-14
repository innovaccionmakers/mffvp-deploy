using Common.SharedKernel.Domain;

namespace People.Domain.EconomicActivities;
public static class EconomicActivityErrors
{
    public static Error NotFound(string economicactivityId) =>
        Error.NotFound(
            "EconomicActivity.NotFound",
            $"The economicactivity with identifier {economicactivityId} was not found"
        );
}