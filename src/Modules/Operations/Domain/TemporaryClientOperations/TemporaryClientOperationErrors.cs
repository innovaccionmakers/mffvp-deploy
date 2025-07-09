using Common.SharedKernel.Domain;

namespace Operations.Domain.TemporaryClientOperations;

public static class TemporaryClientOperationErrors
{
    public static Error NotFound(long temporaryClientOperationId)
    {
        return Error.NotFound(
            "TemporaryClientOperation.NotFound",
            $"The temporary client operation with identifier {temporaryClientOperationId} was not found"
        );
    }
}
