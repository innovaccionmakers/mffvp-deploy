using Common.SharedKernel.Domain;

namespace Products.Domain.Plans;

public static class PlanErrors
{
    public static Error NotFound(long planId)
    {
        return Error.NotFound(
            "Plan.NotFound",
            $"The plan with identifier {planId} was not found"
        );
    }
}