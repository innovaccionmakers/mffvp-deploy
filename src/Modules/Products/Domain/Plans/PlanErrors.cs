using Common.SharedKernel.Core.Primitives;

namespace Products.Domain.Plans;

public static class PlanErrors
{
    public static Error NotFound(int planId)
    {
        return Error.NotFound(
            "Plan.NotFound",
            $"The plan with identifier {planId} was not found"
        );
    }
}