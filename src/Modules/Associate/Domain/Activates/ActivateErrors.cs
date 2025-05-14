using Common.SharedKernel.Domain;

namespace Associate.Domain.Activates;

public static class ActivateErrors
{
    public static Error NotFound(int activateId)
    {
        return Error.NotFound(
            "Activate.NotFound",
            $"The activate with identifier {activateId} was not found"
        );
    }

    public static Error NotFound(string dentification) =>
        Error.NotFound(
            "Activate.NotFound",
            $"The activate with identifier {dentification} was not found"
        );
}