using Common.SharedKernel.Domain;

namespace Contributions.Domain.TrustOperations;
public static class TrustOperationErrors
{
    public static Error NotFound(Guid trustoperationId) =>
        Error.NotFound(
            "TrustOperation.NotFound",
            $"The trustoperation with identifier {trustoperationId} was not found"
        );
}