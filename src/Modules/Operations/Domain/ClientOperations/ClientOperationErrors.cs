using Common.SharedKernel.Core.Primitives;

namespace Operations.Domain.ClientOperations;

public static class ClientOperationErrors
{
    public static Error NotFound(long clientoperationId)
    {
        return Error.NotFound(
            "ClientOperation.NotFound",
            $"The clientoperation with identifier {clientoperationId} was not found"
        );
    }
}