using Common.SharedKernel.Core.Primitives;

namespace Products.Domain.Objectives;

public static class ObjectiveErrors
{
    public static Error NotFound(int objectiveId)
    {
        return Error.NotFound(
            "Objective.NotFound",
            $"The objective with identifier {objectiveId} was not found"
        );
    }
}