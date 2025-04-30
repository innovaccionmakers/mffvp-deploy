using Common.SharedKernel.Domain;

namespace Contributions.Domain.ClientOperations;
public static class ClientOperationErrors
{
    public static Error NotFound(Guid clientoperationId) =>
        Error.NotFound(
            "ClientOperation.NotFound",
            $"The clientoperation with identifier {clientoperationId} was not found"
        );
}