using Common.SharedKernel.Domain;

namespace Products.Domain.Objectives;
public static class ObjectiveErrors
{
    public static Error NotFound(long objectiveId) =>
        Error.NotFound(
            "Objective.NotFound",
            $"The objective with identifier {objectiveId} was not found"
        );
}